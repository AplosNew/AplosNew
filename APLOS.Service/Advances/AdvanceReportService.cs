using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Currencies;
using Library.Service.Extension.Accounts;
using Library.Service.Helpers;
using Library.Service.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;

namespace Library.Service.Advances
{
    public class AdvanceReportService : IAdvanceReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IAdvanceService _advanceService;
        private readonly IAdvanceWriteOffService _advanceWriteOffService;
        private readonly IPlantService _plantService;

        public AdvanceReportService(ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IAdvanceWriteOffService advanceWriteOffService
            , IAdvanceService advanceService
            , IPlantService plantService
            )
        {
            _sqlRepository = sqlRepository;
            _advanceService = advanceService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _advanceWriteOffService = advanceWriteOffService;
            _plantService = plantService;
        }

        public IWorkbook GetVendorAdvanceWriteOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var headerData = _advanceWriteOffService.GetAdvanceWriteOffReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.VendorAdvanceWriteOff);

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

            report.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            report.SetText(ref sheet, row, 2, headerData["PartyName"].ToString()); row++;

            report.SetMasterHeaderText(ref sheet, row, 1, "Vendor Plant");
            report.SetText(ref sheet, row, 2, headerData["PartyPlantName"].ToString()); row++;

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

            var _rowL = 12;
            var headreColIndex = 1;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 32); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 32); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 32); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Particulars", 32); headreColIndex++;

            var sumdrcrCol = headreColIndex;
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, headerData["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
                sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].BorderAround(ExcelLineStyle.Thin);
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", 12);
                headreColIndex++;
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);
                headreColIndex++;
            }
            double _Total_Amount = 0;
            report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
            sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].BorderAround(ExcelLineStyle.Thin);
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);

            var shet2EndxlsCol = headreColIndex;
            double vAmount = 0;
            var col = 1;
            var Row_Total_Start = _rowL + 1;

            var data = _advanceWriteOffService.GetAdvanceWriteOffReportData(companyId, voucherId);



            for (int n = 0; n < data.Count; n++)
            {
                _rowL++;
                col = 1;
                report.SetText(ref sheet, _rowL, col, data[n]["GLGeneralInfoCode"] + " - " + data[n]["GLGeneralInfoName"]); col++;
                report.SetText(ref sheet, _rowL, col, data[n]["BudgetName"].ToString()); col++;
                report.SetText(ref sheet, _rowL, col, data[n]["ActivityName"].ToString()); col++;
                report.SetText(ref sheet, _rowL, col, data[n]["Particulars"].ToString()); col++;
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
            report.SetText(ref sheet, _rowL + 1, 1, "In Word:", true);
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
            _rowL++;
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
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, headerData["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            return workbook;


        }

        public IWorkbook GetCustomerAdvanceWriteOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var headerData = _advanceWriteOffService.GetAdvanceWriteOffReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.CustomerAdvanceWriteOff);

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

            report.SetMasterHeaderText(ref sheet, row, 1, "Vendor");
            report.SetText(ref sheet, row, 2, headerData["PartyName"].ToString());
            row++;


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

            var data = _advanceWriteOffService.GetAdvanceWriteOffReportData(companyId, voucherId);
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
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, headerData["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            return workbook;
        }
     
        public IWorkbook GetAdvanceReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Bank Journal";

            // var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.AdvanceJournalVoucher);
            //var header = _bankJournalService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            AdvanceExtensionService advanceExtensionService = new AdvanceExtensionService();

            var header = advanceExtensionService.GetCustomerAdvanceHeader(companyGroupId, companyId, plantId, voucherId, sourceType);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            //  var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);
            var dsLocal = advanceExtensionService.GetCustomerAdvanceDetailData(voucherId);//companyGroupId, companyId, plantId, voucherId, SourceType.VendorAdvance

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;

            int colBaseCurrencyDebit = 0;
            int colBaseCurrencyCredit = 0;
            int colTranCurrencyDebit = 0;
            int colTranCurrencyCredit = 0;

            int colVoucherNo = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherNo, "Voucher No");
            sheet[row, colVoucherNo].ColumnWidth = 18;
            sheet.Range[row, colVoucherNo].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;
            int colVoucherNoValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherNoValue, header["VoucherNo"].ToString());
            sheet[row, colVoucherNoValue].ColumnWidth = 12;
            sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            xlsCol++; //3
            int colReceived = xlsCol;
            xlsCol++;//4
            sheet[row, xlsCol].ColumnWidth = 10;

            xlsCol++; //5
            int colParticulars = xlsCol;

            xlsCol++;//6
            int colVoucherDate = xlsCol;
            reportUtility.SetMasterHeaderText(ref sheet, row, colVoucherDate, "Voucher Date");
            sheet.Range[row, colVoucherDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            xlsCol++;//7
            int colVoucherDateValue = xlsCol;
            reportUtility.SetText(ref sheet, row, colVoucherDateValue, header["VoucherDate"].ToString());
            sheet.Range[row, colVoucherDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            

            int colPostingDate = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPostingDate, "Posting Date");
            sheet.Range[row, colPostingDate].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colPostingDateValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colPostingDateValue, header["PostingDate"].ToString());
            sheet.Range[row, colPostingDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            int colDocDate = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocDate, "DocDate");
            sheet.Range[row, colDocDate].VerticalAlignment = ExcelVAlign.VAlignTop;
            int colDocDateValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocDateValue, header["DocDate"].ToString());
            sheet.Range[row, colDocDateValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;
            //CustomerPlant

            int colCheckNo = colVoucherNo;
            int colCheckNoValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckNo, "CheckNo");
            reportUtility.SetText(ref sheet, row, colCheckNoValue, header["CheckNumber"].ToString());


            int colCheckDate = colVoucherDate;
            int colCheckDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());
            row++;



            int colParty = colVoucherNo;
            int colPartyValue = colVoucherNoValue;
            if (sourceType == SourceType.CustomerAdvance)
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Customer:");
                reportUtility.SetText(ref sheet, row, colPartyValue, header["CustomerPlant"].ToString());
            }
            if (sourceType == SourceType.VendorAdvance)
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Vendor:");
                reportUtility.SetText(ref sheet, row, colPartyValue, header["CustomerPlant"].ToString());
            }

            int colDocRef = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRef, "Doc Ref");
            sheet.Range[row, colDocRef].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocRefValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocRefValue, header["DocRefNo"].ToString());
            sheet.Range[row, colDocRefValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colNaration = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNaration, "Narration");
            int colNarationValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colNarationValue, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

            sheet.Range[row, colNaration].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colNarationValue].VerticalAlignment = ExcelVAlign.VAlignTop;

            

            int colStatus = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            int colStatusValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());
            sheet.Range[row, colStatus].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colStatusValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            int colPONo = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colPONo, "PO No");
            sheet.Range[row, colPONo].VerticalAlignment = ExcelVAlign.VAlignTop;
            int colPONoValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colPONoValue, header["PONo"].ToString());
            sheet.Range[row, colPONoValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            //int colPONo = colVoucherNo;
            //reportUtility.SetMasterHeaderText(ref sheet, row, colPONo, "PO No");
            //sheet.Range[row, colPONo].VerticalAlignment = ExcelVAlign.VAlignTop;
            //int colPONoValue = colVoucherNoValue;
            //reportUtility.SetText(ref sheet, row, colPONoValue, header["PONo"].ToString());
            //sheet.Range[row, colPONoValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            //row++;




            row++;  //10

            colTranCurrencyDebit = colVoucherDate; //col6
            colTranCurrencyCredit = colVoucherDateValue; //7
            xlsCol++; //8 
            colBaseCurrencyDebit = xlsCol;
            xlsCol++; //9 
            colBaseCurrencyCredit = xlsCol;

            colLast = companyCurrencyId == transcationCurrency ? colTranCurrencyCredit : colBaseCurrencyCredit;
            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, colTranCurrencyDebit, row, colTranCurrencyCredit].Merge();

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, colBaseCurrencyDebit, row, colBaseCurrencyCredit].Merge();
            }
            //sheet[row, 6].RowHeight = 15;

            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;


            int colGl = colVoucherNo;
            reportUtility.SetHeaderText(ref sheet, row, colGl, "GL");
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();


            //reportUtility.SetHeaderText(ref sheet, row, colParticulars, "Particulars");
            //sheet[row, colParticulars].ColumnWidth = 23;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight); //colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, colBaseCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight); //colusdCradit = xlsCol;
                colLast = colBaseCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyDebit, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, colTranCurrencyCredit, "Credit", 13, ExcelHAlign.HAlignRight);
                colLast = colTranCurrencyCredit;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;

                formulaStartRow = row;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    // glName = string.Empty;

                    var glName = dsLocal.Rows[i]["Budget"].ToString();

                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["ActivityName"]); //Activity

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 4) + row].Merge();

                    // reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colBaseCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colTranCurrencyDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colTranCurrencyCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);

                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, colReceived, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colBaseCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colBaseCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colBaseCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colBaseCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colBaseCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colBaseCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colBaseCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colTranCurrencyDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colTranCurrencyCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colTranCurrencyCredit) + (formulaEndRow) + ")";
                    sheet.Range[row, colTranCurrencyCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colTranCurrencyCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colTranCurrencyCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colTranCurrencyCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colTranCurrencyCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colTranCurrencyDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, colGl, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                    sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;
                    row++;

                }

                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row].CellStyle.Font.Bold = true;
                sheet.Range[row, colVoucherNoValue].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.UsedRange.AutofitColumns();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, colVoucherNo, header["AddedBy"].ToString());
                sheet.Range[row, colVoucherNo].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colVoucherNo, "Prepared By", true);
                sheet[row, colVoucherNo].ColumnWidth = 18;


                reportUtility.SetTextMiddle(ref sheet, row, colReceived, "Received By", true);
                sheet[row, colReceived].ColumnWidth = 14;
                sheet.Range[row, colReceived].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                reportUtility.SetSignatureText(ref sheet, row - 1, colParticulars, header["PostedBy"].ToString());
                sheet.Range[row, colParticulars].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, colParticulars, "Checked By", true);
                sheet[row, colParticulars].ColumnWidth = 15;


                reportUtility.SetTextMiddle(ref sheet, row, colTranCurrencyCredit, "Authorized By", true);
                sheet[row, colTranCurrencyDebit].ColumnWidth = 15;
                sheet[row, colTranCurrencyCredit].ColumnWidth = 15;
                sheet.Range[row, colTranCurrencyCredit].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                sheet[row, colBaseCurrencyDebit].ColumnWidth = 15;
                sheet[row, colBaseCurrencyCredit].ColumnWidth = 15;
                if (sourceType == SourceType.CustomerAdvance)
                {
                    reportUtility.CompanyPlantHeader(ref sheet, colLast, "Customer Advance", companyId, plantId, plantName, null);
                }
                if (sourceType == SourceType.VendorAdvance)
                {
                    reportUtility.CompanyPlantHeader(ref sheet, colLast, "Vendor Advance", companyId, plantId, plantName, null);
                }


                //reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId,plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;



                if (sourceType == SourceType.CustomerAdvance)
                {
                    reportUtility.CompanyPlantHeader(ref sheet, colLast, "Customer Advance", companyId, plantId, plantName, null);
                }
                if (sourceType == SourceType.VendorAdvance)
                {
                    reportUtility.CompanyPlantHeader(ref sheet, colLast, "Vendor Advance", companyId, plantId, plantName, null);
                }

                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        private DataTable GetAdvanceData(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, VDC.VoucherDetailId, V.VoucherNo, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo,  V.DocRefNo, [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END
                            , REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate
                            , UPPER(V.Narration) AS Narration, V.CurrencyId, CU.Code AS CurrencyCode, V.AddedBy, V.PostedBy, VD.DrAmount AS TDrAmount, VD.CrAmount AS TCrAmount, VDC.DrAmount, VDC.CrAmount
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, BUD.UserName AS Budget, ACT.UserName AS Activity, BM.AccountTitle AS BankMasterName
                            , P.UserName AS PartyName, PP.UserName AS PartyPlantName, EI.EmployeeCode, EI.[EmployeeName] AS [Employee], CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[AdvanceDetail] AS CID ON CID.Id=VD.AdvanceDetailId
                            LEFT JOIN [TRN].[Advance] AS CI ON CI.Id=CID.AdvanceId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=CI.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                            LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=CI.EmployeeId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "' AND V.Id='" + voucherId + "'";
            return _sqlRepository.GetDataTable(cmdText);
        }

        private DataTable GetAdvanceSetOffData(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, Replace(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, Replace(CONVERT(VARCHAR(11), v.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.Narration, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount AS TDrAmount,VD.CrAmount AS TCrAmount, VDC.DrAmount, VDC.CrAmount, V.SourceType, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL
                            , GL.AccountCode AS GLGeneralInfoCode,GL.AccountCode+' - '+ BM.AccountTitle AS BankMain, P.Code AS CustomerCode, P.UserName AS Customer, BUD.UserName AS Budget, ACT.UserName AS Activity
                            , EI.[EmployeeName] AS [Employee], PL.UserName AS PlantName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[AdvanceWriteOffDetail] AS CID ON CID.Id=VD.AdvanceWriteOffDetailId
                            LEFT JOIN [TRN].[AdvanceWriteOff] AS CI ON CI.Id=CID.AdvanceWriteOffId
                            LEFT JOIN [TRN].[AdvanceDetail] AS AD ON AD.Id=VD.AdvanceDetailId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=CI.PartyId
                            LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=CI.EmployeeId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=VD.BankMasterId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [ORG].[Plant] AS PL ON PL.Id=AD.PlantId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "' AND V.Id = '" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        private DataTable GetInvoiceChargeData(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, Replace(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, Replace(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, Replace(CONVERT(VARCHAR(11), v.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.Narration, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount AS TDrAmount,VD.CrAmount AS TCrAmount, VDC.DrAmount, VDC.CrAmount, V.SourceType, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL
                            , GL.AccountCode AS GLGeneralInfoCode,GL.AccountCode+' - '+ BM.AccountTitle AS BankMain, P.Code AS CustomerCode, P.UserName AS Customer, BUD.UserName AS Budget, ACT.UserName AS Activity
                            , EI.[EmployeeName] AS [Employee], PL.UserName AS PlantName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[InvoiceWriteOffDetail] AS CID ON CID.Id=VD.AdvanceWriteOffDetailId
                            LEFT JOIN [TRN].[AdvanceWriteOff] AS CI ON CI.Id=CID.AdvanceWriteOffId
                            LEFT JOIN [TRN].[AdvanceDetail] AS AD ON AD.Id=VD.AdvanceDetailId
                            LEFT JOIN [HKP].[Party] AS P ON P.Id=CI.PartyId
                            LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=CI.EmployeeId
                            LEFT JOIN [MST].[BankMaster] AS BM ON BM.id=VD.BankMasterId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id = BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [ORG].[Plant] AS PL ON PL.Id=AD.PlantId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.SourceType='" + sourceType + "' AND V.Id = '" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }

     
        public IWorkbook GetEmployeeAdvanceReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = EmployeeAdvanceReportHeader(companyGroupId, companyId, plantId, voucherId, sourceType);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetAdvanceData(voucherId);
            var dsLocalARS = GetAdvanceReqSheduleData(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colPurpose = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            sheet[row, 1].ColumnWidth = 20;
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            sheet[row, 2].ColumnWidth = 20;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Voucher Date");
            sheet[row, 6].ColumnWidth = 25;
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            reportUtility.SetText(ref sheet, row, 7, header["VoucherDate"].ToString());
            sheet[row, 7].ColumnWidth = 25;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "DocDate");
            reportUtility.SetText(ref sheet, row, 7, header["DocDate"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Employee");
            reportUtility.SetText(ref sheet, row, 2, header["EmployeeName"].ToString());
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 7, header["DocRefNo"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Status");
            reportUtility.SetText(ref sheet, row, 7, header["Status"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 7 : 9;
            
            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 6, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 8, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 8, row, 9].Merge();
            }
            sheet.Range[row, 6, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 6, row, colLast].BorderInside(ExcelLineStyle.Hair);

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++; /*xlsCol++;*/
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            //xlsCol++; //clo3


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Purpose"); colPurpose = xlsCol;
            sheet[reportUtility.GetColumnNameForXls(colPurpose) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge(); 
            xlsCol++;
            xlsCol++;
            xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
            }

            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;

                formulaStartRow = row;

                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    reportUtility.SetText(ref sheet, row, colPurpose, dsLocal.Rows[i]["Purpose"].ToString());
                    sheet[row, 3].ColumnWidth = 25;
                    //sheet[row, colPurpose].WrapText = true;

                    sheet[reportUtility.GetColumnNameForXls(colPurpose) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
                    //xlsCol++;

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

                }
                formulaEndRow = row - 1;

                reportUtility.SetText(ref sheet, row, 5, "Total: ", true);
                sheet[row, 5].ColumnWidth = 20;
                sheet[row, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, colinrDebit, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, colinrDebit, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

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

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet.Range[row, 1].ColumnWidth = 21;

                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Received By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 5, header["CheckedBy"].ToString());
                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Checked By", true);
                //sheet.Range[row, 5].ColumnWidth = 11;
                
                sheet.Range[row, 6].ColumnWidth = 21;

                reportUtility.SetSignatureText(ref sheet, row - 1, 7, header["ApprovedBy"].ToString());
                sheet.Range[row, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 7, "Approved By", true);
                sheet.Range[row, 7].ColumnWidth = 21;
                


                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }


            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            if (dsLocalARS.Rows.Count > 0)
            {
                row++;
                row++;
                row++;
                int COL = 1;
                int ROW = row;

                sheet[ROW, COL].Text = "Installment Date";
                int colInstallmentDate = COL;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet[ROW, COL].ColumnWidth = 20;
                COL++;

                sheet[ROW, COL].Text = "Installment No";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                int colInstallmentNo = COL;
                sheet[ROW, COL].ColumnWidth = 20;
                COL++;

                sheet[ROW, COL].Text = "Installment Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colInstallmentAmount = COL;
                sheet[ROW, COL].ColumnWidth = 17;
                COL++;

                sheet[ROW, COL].Text = "Profit Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProfitAmount = COL;
                sheet[ROW, COL].ColumnWidth = 17;
                COL++;


                sheet[ROW, COL].Text = "Principal Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPrincipalAmount = COL;
                sheet[ROW, COL].ColumnWidth = 17;
                COL++;

                sheet[ROW, COL].Text = "Balance";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBalance = COL;
                sheet[ROW, COL].ColumnWidth = 17;
                COL++;

                sheet[ROW, COL].Text = "YearNo";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                int colYearNo = COL;
                sheet[ROW, COL].ColumnWidth = 10;
                COL++;

                sheet[ROW, COL].Text = "MonthNo";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                int colMonthNo = COL;
                sheet[ROW, COL].ColumnWidth = 10;
                COL++;

                sheet[ROW, COL].Text = "Status";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                int colPaymentStatus = COL;
                sheet[ROW, COL].ColumnWidth = 8;
                //COL++;

                ROW++;

                int endCol = COL;

                int StartDataRow = ROW;

                for (int i = 0; i < dsLocalARS.Rows.Count; i++)
                {
                    sheet[ROW, colInstallmentDate].DateTime = Convert.ToDateTime(dsLocalARS.Rows[i]["InstallmentDate"].ToString());
                    sheet[ROW, colInstallmentDate].NumberFormat = "dd-MMM-yyyy";
                    sheet[ROW, colInstallmentDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet[ROW, colInstallmentNo].Text = dsLocalARS.Rows[i]["InstallmentNo"].ToString();
                    sheet[ROW, colInstallmentNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet[ROW, colInstallmentAmount].Number = clsStaticInfo.dbl(dsLocalARS.Rows[i]["InstallmentAmount"].ToString());
                    sheet[ROW, colInstallmentAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet[ROW, colProfitAmount].Number = clsStaticInfo.dbl(dsLocalARS.Rows[i]["ProfitAmount"].ToString());
                    sheet[ROW, colProfitAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet[ROW, colPrincipalAmount].Number = clsStaticInfo.dbl(dsLocalARS.Rows[i]["PrincipalAmount"].ToString());
                    sheet[ROW, colPrincipalAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                   
                    sheet[ROW, colBalance].Number = clsStaticInfo.dbl(dsLocalARS.Rows[i]["Balance"].ToString());
                    sheet[ROW, colBalance].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet[ROW, colYearNo].Text = dsLocalARS.Rows[i]["YearNo"].ToString();
                    sheet[ROW, colYearNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet[ROW, colMonthNo].Text = dsLocalARS.Rows[i]["MonthNo"].ToString();
                    //sheet[ROW, colMonthNo].Text = dsLocalARS.Rows[i]["MonthNo"].ToString();
                    sheet[ROW, colMonthNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet[ROW, colPaymentStatus].Text = dsLocalARS.Rows[i]["Status"].ToString();
                    //sheet[ROW, colMonthNo].Text = dsLocalARS.Rows[i]["MonthNo"].ToString();
                    sheet[ROW, colPaymentStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                    ROW++;
                }


                sheet[StartDataRow - 1, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet[StartDataRow - 1, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);
            }


            return workbook;
        }

        private Dictionary<string, object> EmployeeAdvanceReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, U.FullName AddedBy,isnull(PC.EmployeeName,UP.FullName) CheckedBy,PA.EmployeeName ApprovedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            ,EmployeeName=case when P.EmployeeName<>'' then P.EmployeeName else  ESAE.EmployeeName End  , BJ.CurrencyId, C.Code AS CurrencyCode,EAR.Remarks Purpose
                            FROM [TRN].[Voucher] AS V
                            LEFT JOIN [TRN].[Advance] AS BJ  ON V.Id=BJ.VoucherId
                            LEFT JOIN TRN.EmployeeSalaryAdvance AS ESA  ON V.Id=ESA.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [DBO].[EmployeeInformation] AS P ON P.SystemId=BJ.EmployeeId
							LEFT JOIN [DBO].[EmployeeInformation] AS ESAE ON ESAE.SystemId=ESA.EmployeeId
							left join TRN.EmployeeAdvanceRequisition EAR ON EAR.SystemId=BJ.RequisitionId
							LEFT JOIN [DBO].[EmployeeInformation] AS PA ON PA.SystemId=EAR.ApprovedBy
							 LEFT JOIN [DBO].[EmployeeInformation] AS PC ON PC.SystemId=EAR.CheckedBy
							 LEFT JOIN [SEC].[User] AS U ON U.UserId=V.AddedBy
							LEFT JOIN [SEC].[User] AS UP ON UP.UserId=V.PostedBy
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetAdvanceData(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, U.FullName AddedBy,isnull(PC.EmployeeName,UP.FullName) CheckedBy ,  VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.EmployeeName , VD.Narration AS DetailNarration, BUD.UserName AS Budget
							,EAR.Remarks Purpose,PA.EmployeeName ApprovedBy
	                        ,Activity = case when vd.CashMasterId<>'' then cm.UserName when VD.BankMasterId<>'' then BM.AccountTitle else ACT.UserName end

                            , CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[AdvanceDetail] AS IVD ON IVD.Id=VD.AdvanceDetailId
                            LEFT JOIN [TRN].[Advance] AS IV ON IV.Id=IVD.AdvanceId
                            LEFT JOIN [DBO].[EmployeeInformation] AS P ON P.SystemId=IV.EmployeeId
							left join TRN.EmployeeAdvanceRequisition EAR ON EAR.SystemId=IV.RequisitionId
							 LEFT JOIN [DBO].[EmployeeInformation] AS PA ON PA.SystemId=EAR.ApprovedBy
							  LEFT JOIN [DBO].[EmployeeInformation] AS PC ON PC.SystemId=EAR.CheckedBy
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            left join [MST].[BankMaster] BM on BM.Id=VD.BankMasterId
							LEFT JOIN [SEC].[User] AS U ON U.UserId=V.AddedBy
							LEFT JOIN [SEC].[User] AS UP ON UP.UserId=V.PostedBy
                            WHERE V.Archive=0 AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetEmployeeAdvanceReportPortal(string companyGroupId, string companyId, string plantId, string plantName, string employeeAdvanceRequisitionId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "EmployeeAdvance";

            var headerData = EmployeeAdvanceReportHeaderPortal(companyGroupId, companyId, plantId, employeeAdvanceRequisitionId);

            //report.SetText(ref sheet, 5, 3, "MONEY REQUISITION");
            //sheet[5, 3]. = true;
            var _row = 6;
            var shet2EndxlsCol = 1;
                       
            report.SetMasterHeaderText(ref sheet, _row, 1, "Employee");
            report.SetText(ref sheet, _row, 2, headerData["EmployeeName"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;

            report.SetMasterHeaderText(ref sheet, _row, 1, "Requisition No");
            report.SetText(ref sheet, _row, 2, employeeAdvanceRequisitionId);
            _row++;

            var _rowL = 11;

            
            var _rowR = 5;

            
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Status");
            report.SetText(ref sheet, _rowR, 5, headerData["ApprovalStatus"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();
            sheet.Range[_row, 4].VerticalAlignment = ExcelVAlign.VAlignTop;

            var headreColIndex = 1;

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Requisition Date", 14, ExcelHAlign.HAlignCenter);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Requisition Required Date", 20, ExcelHAlign.HAlignCenter);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Advance Type", 12, ExcelHAlign.HAlignCenter);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Remarks", 30, ExcelHAlign.HAlignCenter);
            headreColIndex++;

            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            double _Total_Amount = 0;
            report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, companyCurrencyCode, 11, ExcelHAlign.HAlignCenter);

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Amount", ExcelHAlign.HAlignCenter);
            sheet.Range[_rowL, headreColIndex].ColumnWidth = 18;
            shet2EndxlsCol = headreColIndex;

            double vAmount = 0;
            var data = GetEmployeeAdvanceDataPortal(employeeAdvanceRequisitionId);

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < data.Count; n++)
            {
                _rowL++;
                var drcrCol = 1;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["RequisitionAddedDate"].ToString()); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["RequisitionRequiredDate"].ToString()); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["AdvanceType"].ToString()); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["Remarks"].ToString());
                sheet[_rowL, drcrCol].RowHeight = 27;
                sheet[_rowL, drcrCol].WrapText = true;
                drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(data[n]["Amount"].ToString())); drcrCol++;
                _Total_Amount += Convert.ToDouble(data[n]["Amount"].ToString());
            }

            _rowL++;

            report.SetText(ref sheet, _rowL, 1, "Total :", true);
            sheet[_rowL, 1, _rowL, shet2EndxlsCol - 1].Merge();

            sheet.Range[_rowL, shet2EndxlsCol - 1].Formula = "=SUM(" + report.GetColumnNameForXls(shet2EndxlsCol - 1) + Row_Total_Start + ":" + report.GetColumnNameForXls(shet2EndxlsCol - 1) + (_rowL - 1) + ")";
            sheet.Range[_rowL, shet2EndxlsCol - 1].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, shet2EndxlsCol - 1].CellStyle.Font.Bold = true;
            sheet.Range[_rowL, shet2EndxlsCol - 1].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[_rowL, shet2EndxlsCol].Formula = "=SUM(" + report.GetColumnNameForXls(shet2EndxlsCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + (_rowL - 1) + ")";
            sheet.Range[_rowL, shet2EndxlsCol].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, shet2EndxlsCol].CellStyle.Font.Bold = true;
            sheet.Range[_rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

            vAmount = vAmount / 2;
            _rowL += 1;
            _rowL++;
            report.SetText(ref sheet, _rowL, 1, "In Word:", true);
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(_Total_Amount, companyCurrencyId);
            sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;
            _rowL++;
            _rowL++;
            _rowL++;
            _rowL++;
            _rowR= _rowL;
            report.SetSignatureText(ref sheet, _rowL - 1, 2, headerData["CheckedBy"].ToString());
            sheet.Range[_rowL, 2].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 2, "Checked By", true);
            //sheet[_rowL, 2].ColumnWidth = 25;

            report.SetSignatureText(ref sheet, _rowR - 1, 4, headerData["ApprovedBy"].ToString());
            //sheet.Range[_rowR, 4].ColumnWidth = 18;
            sheet.Range[_rowR, 4].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowR, 4, "Approved By", true);

            _rowL = _rowL + 4;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, "Employee Advance Money Requisition", companyId, plantId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            return workbook;
        }
        private Dictionary<string, object> EmployeeAdvanceReportHeaderPortal(string companyGroupId, string companyId, string plantId, string employeeAdvanceRequisitionId)
        {
            var cmdText = @"SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate,EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy  
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.SystemId = '" + employeeAdvanceRequisitionId + "'";
            return _sqlRepository.GetData(cmdText);
        }
        public List<Dictionary<string, object>> GetEmployeeAdvanceDataPortal(string employeeAdvanceRequisitionId)
        {
            var sql = @"SELECT CURR.Code CurrencyCode, CURR.Id CurrencyId,EEI.EmployeeName ,Format(EAR.RequisitionAddedDate,'dd-MMM-yyyy') RequisitionAddedDate,
                            Format(EAR.RequisitionRequiredDate,'dd-MMM-yyyy') RequisitionRequiredDate,EAR.AdvanceType,  EAR.Remarks,EAR.Amount,EAR.SystemId, EEC. EmployeeName CheckedBy,EAR.ApprovalStatus, EEA. EmployeeName ApprovedBy  
                            FROM [TRN].[EmployeeAdvanceRequisition] EAR 
                            LEFT JOIN SCS.Currency CURR ON CURR.Id = EAR.CurrencyId
                            LEFT JOIN EmployeeInformation EEI ON EEI.SystemId = EAR.EmpSystemId
                            LEFT JOIN EmployeeInformation EEC ON EEC.SystemId = EAR.CheckedBy
                            LEFT JOIN EmployeeInformation EEA ON EEA.SystemId = EAR.ApprovedBy
                            WHERE EAR.SystemId = '" + employeeAdvanceRequisitionId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }
        private DataTable GetAdvanceReqSheduleData(string voucherId)
        {
            try
            {
                var sql = @"select ESA.VoucherId, ESA.EmployeeId,ESA.PartyType,ESA.TransactionType,ESA.SourceType,ESA.VoucherDate,ESA.PostingDate,ESA.DocDate,esa.DocRefNo
                            ,ESA.Narration,ESA.Amount,ARS.Id AdvanceReqScheduleId,ARS.InstallmentDate,ARS.InstallmentNo, ARS.InstallmentAmount,ARS.ProfitAmount
                            ,ARS.PrincipalAmount,ARS.OtherAmount,ars.TaxAmount,ARS.Balance, ARS.YearNo
							
							,MonthNo = case when ARS.MonthNo=1 then 'January'
							           when ARS.MonthNo=2 then 'February'
							           when ARS.MonthNo=3 then 'March'
							           when ARS.MonthNo=4 then 'April'
							           when ARS.MonthNo=5 then 'May'
							           when ARS.MonthNo=6 then 'Jun'
							           when ARS.MonthNo=7 then 'July'
							           when ARS.MonthNo=8 then 'Augest'
							           when ARS.MonthNo=9 then 'September'
							           when ARS.MonthNo=10 then 'October'
							           when ARS.MonthNo=11 then 'November'
							           when ARS.MonthNo=12 then 'December'
									else '' end,
									AW.Amount SetOffAmount
                        ,[Status]=case when AW.Amount>0 then 'Paid' else '' end
                          from TRN.EmployeeSalaryAdvance ESA 
                            LEFT JOIN dbo.AdvanceReqSchedule ARS ON ESA.Id = ARS.EmployeeSalaryAdvanceId
							left join TRN.EmployeeAdvanceDeduction EAD ON EAD.AdvanceReqScheduleId=ARS.Id 
							LEFT JOIN TRN.Advance A ON A.Id=EAD.AdvanceId
							LEFT JOIN (SELECT SUM(Amount) Amount,AdvanceId FROM TRN.AdvanceWriteOffDetail GROUP BY AdvanceId) AW ON AW.AdvanceId=A.Id
                            where ESA.VoucherId='" + voucherId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IWorkbook GetEmployeeAdvanceWriteOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var headerData = _advanceWriteOffService.GetAdvanceWriteOffReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.EmployeeAdvanceWriteOff);

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

            report.SetMasterHeaderText(ref sheet, row, 1, "Employee");
            report.SetText(ref sheet, row, 2, headerData["EmployeeName"].ToString()); row++;

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

            var data = _advanceWriteOffService.GetAdvanceWriteOffReportData(companyId, voucherId);
            for (int n = 0; n < data.Count; n++)
            {
                _rowL++;
                col = 1;
                if (!string.IsNullOrEmpty(data[n]["BankMasterId"].ToString()))
                {
                    report.SetText(ref sheet, _rowL, col, data[n]["GLGeneralInfoCode"] + " - " + data[n]["AccountTitle"]); col++;
                }
                else
                {
                    report.SetText(ref sheet, _rowL, col, data[n]["GLGeneralInfoCode"] + " - " + data[n]["GLGeneralInfoName"]); col++;
                }
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
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, headerData["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            return workbook;
        }

        public IWorkbook GetInterTransactionReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";
            var headData = _advanceService.GetReportHeader(voucherId);
            var advanceDataList = GetAdvanceData(companyGroupId, companyId, plantId, voucherId, sourceType);
            var dvCustomer = new DataView(advanceDataList)
            {
                RowFilter = "PartyName IS NOT NULL"
            };
            var dtCustomer = dvCustomer.ToTable(true, "PartyName");
            var dvLocation = new DataView(advanceDataList)
            {
                RowFilter = "PartyPlantName IS NOT NULL"
            };
            var dtLocation = dvLocation.ToTable(true, "PartyPlantName");
            var dtGeneralVoucher = advanceDataList;
            var tranCurrencyId = dtGeneralVoucher.Rows[0]["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            // Set report Name
            reportFileName = Convert.ToDateTime(dtGeneralVoucher.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dtGeneralVoucher.Rows[0]["VoucherNo"];

            var _col = 1;
            var _row = 5;
            var shet2EndxlsCol = _col;
            const int _col3 = 2;
            var sumdrcrCol = 0;

            report.SetMasterHeaderText(ref sheet, _row, _col, "Voucher No");
            report.SetMiddleAlignmentText(ref sheet, _row, _col3, headData["VoucherNo"].ToString());
            _row++;

            report.SetMasterHeaderText(ref sheet, _row, _col, "Doc No");
            report.SetMiddleAlignmentText(ref sheet, _row, _col3, headData["DocRefNo"].ToString());
            _row++;
            report.SetMasterHeaderText(ref sheet, _row, _col, "Fiscal Year");
            report.SetMiddleAlignmentText(ref sheet, _row, _col3, headData["FiscalYearName"] + " (" + headData["PeriodNo"] + ")");
            _row++;

            report.SetMasterHeaderText(ref sheet, _row, _col, "Party");
            report.SetMiddleAlignmentText(ref sheet, _row, _col3, headData["PartyName"].ToString());
            _row++;
            report.SetMasterHeaderText(ref sheet, _row, _col, "Narration");
            report.SetMiddleAlignmentText(ref sheet, _row, _col3, headData["Narration"].ToString().ToUpper());
            sheet[report.GetColumnNameForXls(_col3) + _row + ":" + report.GetColumnNameForXls(_col3 + 3) + _row].Merge();
            _row++;
            var _rowR = 5;
            const int _colR = 3;
            const int _col8 = 4;

            report.SetMasterHeaderText(ref sheet, _rowR, _colR, "Voucher Date");
            report.SetMiddleAlignmentText(ref sheet, _rowR, _col8, headData["VoucherDate"].ToString());
            sheet[report.GetColumnNameForXls(_col8) + _rowR + ":" + report.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();
            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, _colR, "Doc Date");
            report.SetMiddleAlignmentText(ref sheet, _rowR, _col8, headData["DocDate"].ToString());
            sheet[report.GetColumnNameForXls(_col8) + _rowR + ":" + report.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, _colR, "Posting Date");
            report.SetMiddleAlignmentText(ref sheet, _rowR, _col8, headData["PostingDate"].ToString());
            sheet[report.GetColumnNameForXls(_col8) + _rowR + ":" + report.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();
            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, _colR, "Party Plant");
            report.SetMiddleAlignmentText(ref sheet, _rowR, _col8, headData["PartyPlantName"].ToString());
            sheet[report.GetColumnNameForXls(_col8) + _rowR + ":" + report.GetColumnNameForXls(_col8 + 1) + _rowR].Merge();

            var row = 10;
            var _rowL = 11;
            var headreColIndex = 1;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 32); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 32); headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 24);
            sumdrcrCol = headreColIndex;
            headreColIndex++;
            if (companyCurrencyId != tranCurrencyId)
            {
                report.SetHeaderText(ref sheet, row, headreColIndex, dtGeneralVoucher.Rows[0]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet.Range[row, headreColIndex, row, headreColIndex + 1].Merge();

                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight);
                headreColIndex++;
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);
                headreColIndex++;
            }
            double _Total_Amount = 0;

            report.SetHeaderText(ref sheet, row, headreColIndex, companyCurrencyCode, ExcelHAlign.HAlignCenter);
            sheet.Range[row, headreColIndex, row, headreColIndex + 1].Merge();
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);

            shet2EndxlsCol = headreColIndex;
            double vAmount = 0;
            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < dtGeneralVoucher.Rows.Count; n++)
            {
                _rowL++;
                headreColIndex = 1;
                var bank = dtGeneralVoucher.Rows[n]["BankMasterName"].ToString();
                var cash = dtGeneralVoucher.Rows[n]["CashMasterName"].ToString();
                if (!string.IsNullOrEmpty(bank))
                    report.SetText(ref sheet, _rowL, headreColIndex, dtGeneralVoucher.Rows[n]["GLGeneralInfoCode"] + " - " + bank);
                else if (!string.IsNullOrEmpty(cash))
                    report.SetText(ref sheet, _rowL, headreColIndex, dtGeneralVoucher.Rows[n]["GLGeneralInfoCode"] + " - " + cash);
                else
                    report.SetText(ref sheet, _rowL, headreColIndex, dtGeneralVoucher.Rows[n]["GLGeneralInfoCode"] + " - " + dtGeneralVoucher.Rows[n]["GL"]);
                headreColIndex++;
                report.SetText(ref sheet, _rowL, headreColIndex, dtGeneralVoucher.Rows[n]["Budget"].ToString()); headreColIndex++;
                report.SetText(ref sheet, _rowL, headreColIndex, dtGeneralVoucher.Rows[n]["Activity"].ToString()); headreColIndex++;
                if (companyCurrencyId != tranCurrencyId)
                {
                    report.SetText(ref sheet, _rowL, headreColIndex, Convert.ToDouble(dtGeneralVoucher.Rows[n]["TDrAmount"])); headreColIndex++;
                    report.SetText(ref sheet, _rowL, headreColIndex, Convert.ToDouble(dtGeneralVoucher.Rows[n]["TCrAmount"])); headreColIndex++;
                    vAmount += Convert.ToDouble(dtGeneralVoucher.Rows[n]["TCrAmount"].ToString());
                }
                report.SetText(ref sheet, _rowL, headreColIndex, Convert.ToDouble(dtGeneralVoucher.Rows[n]["DrAmount"].ToString()));
                headreColIndex++;
                report.SetText(ref sheet, _rowL, headreColIndex, Convert.ToDouble(dtGeneralVoucher.Rows[n]["CrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(dtGeneralVoucher.Rows[n]["CrAmount"].ToString());
            }

            _rowL++;
            report.SetText(ref sheet, _rowL, sumdrcrCol, "Total :", true);
            sheet[_rowL, 1, _rowL, sumdrcrCol].Merge();

            if (companyCurrencyId != tranCurrencyId)
            {
                sumdrcrCol++;
                sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                sumdrcrCol++;
                sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
            }
            sumdrcrCol++;
            sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
            sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
            sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

            sumdrcrCol++;
            sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
            sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
            sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
            sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

            shet2EndxlsCol = headreColIndex;
            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

            _rowL++;
            report.SetText(ref sheet, _rowL, _col, "In Word :", true);
            _col = 2;
            if (companyCurrencyId != tranCurrencyId)
            {
                var _amountValue = report.InWord(vAmount, tranCurrencyId);
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amountValue;
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;
                _rowL++;
            }
            var _amount = report.InWord(_Total_Amount, companyCurrencyId);
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].Text = _amount;
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[report.GetColumnNameForXls(_col) + _rowL].CellStyle.Font.Bold = true;

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;

            _rowL = _rowL + 4;
            report.SetSignatureText(ref sheet, _rowL - 1, 1, headData["AddedBy"].ToString());
            sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 1, "Prepared By", true);

            report.SetSignatureText(ref sheet, _rowL - 1, 3, headData["PostedBy"].ToString());
            sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 3, "Checked By", true);

            sheet.Range[_rowL, shet2EndxlsCol].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, shet2EndxlsCol, "Authorized By", true);

            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, headData["VoucherTypeName"].ToString(), companyId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);

            return workbook;
        }

        public IWorkbook GetInterTransactionVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetInterTransactionHeader(voucherId);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetInterTransactionData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Entry Date");
            reportUtility.SetText(sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Inter Plant");
            reportUtility.SetText(ref sheet, row, 2, header["PlantName"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

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


                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                var lastRow = row - 1;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

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
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["CheckedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        private Dictionary<string, object> GetInterTransactionHeader(string voucherId)
        {
            var cmdText = @"SELECT V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate
                        , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.Narration, P.UserName AS PartyName
                        ,PL.UserName PlantName, VD.PlantId , V.CurrencyId, C.Code AS CurrencyCode, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
						, PP.UserName AS PartyPlantName, V.AddedBy, V.PostedBy, VT.UserName AS VoucherTypeName
                        FROM [TRN].[Advance] AS A
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=A.VoucherId
                        LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId=A.VoucherId
                        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=A.PartyId
                        LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=A.PartyPlantId
                        LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                        LEFT JOIN [ORG].[Plant] AS PL ON PL.Id=VD.PlantId
						LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                        WHERE A.Archive=0 AND V.Id='" + voucherId + "' AND VD.PlantId<>''";
            return _sqlRepository.GetData(cmdText);
        }
        public DataTable GetInterTransactionData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                            , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                            , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                            , VD.Id AS BudgetMasterId, BUD.UserName AS BudgetName, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId
							,[ParticularName]=CASE
								WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
								WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
								WHEN P.UserName<>'' THEN P.UserName 
								WHEN CM.UserName<>'' THEN CM.UserName
                                WHEN FAM.UserName<>'' THEN FAM.UserName
								ELSE ''	END
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							LEFT JOIN [TRN].[AdvanceDetail] AS CID ON CID.Id=VD.AdvanceDetailId
                            LEFT JOIN [TRN].[Advance] AS CI ON CI.Id=CID.AdvanceId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                            LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                            LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
							LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
							LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
							LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
							LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
							LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetInvoiceChargeReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string reportName, SourceType sourceType)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";
            var advanceDataList = GetInvoiceChargeData(companyGroupId, companyId, plantId, voucherId, sourceType);
            var dtGeneralVoucher = advanceDataList;
            var tranCurrencyId = dtGeneralVoucher.Rows[0]["CurrencyId"].ToString();
            var tranCurrencyCode = dtGeneralVoucher.Rows[0]["CurrencyCode"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var lastColumn = 5;
            using (var dvParallelCurrency = new DataView(advanceDataList))
            {
                var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                using (var dvCustomer = new DataView(advanceDataList)
                {
                    Sort = "Customer DESC"
                })
                {
                    var dtCustomer = dvCustomer.ToTable(true, "Customer");
                    var dvMainBody = new DataView(advanceDataList);
                    var dtMainBody = dvMainBody.ToTable(true, "VoucherDetailId", "Park/Post", "GLGeneralInfoCode", "PlantName", "GL", "Budget", "Activity", "TrnCurrency", "BankMain", "DrAmount", "CrAmount", "TDrAmount", "TCrAmount");
                    var _row = 5;
                    var shet2EndxlsCol = 1;

                    // Set report Name
                    reportFileName = Convert.ToDateTime(dtGeneralVoucher.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dtGeneralVoucher.Rows[0]["VoucherNo"];

                    report.SetMasterHeaderText(ref sheet, _row, 1, "Voucher No");
                    report.SetText(ref sheet, _row, 2, dtGeneralVoucher.Rows[0]["VoucherNo"].ToString());
                    _row++;
                    report.SetMasterHeaderText(ref sheet, _row, 1, "Doc Date");
                    report.SetText(ref sheet, _row, 2, dtGeneralVoucher.Rows[0]["DocDate"].ToString());
                    _row++;
                    report.SetMasterHeaderText(ref sheet, _row, 1, "Posting Date");
                    report.SetText(ref sheet, _row, 2, dtGeneralVoucher.Rows[0]["PostingDate"].ToString());
                    _row++;
                    report.SetMasterHeaderText(ref sheet, _row, 1, "Customer");
                    report.SetText(ref sheet, _row, 2, dtCustomer.Rows[0]["Customer"].ToString());
                    _row++;
                    report.SetMasterHeaderText(ref sheet, _row, 1, "Narration");
                    report.SetText(ref sheet, _row, 2, dtGeneralVoucher.Rows[0]["Narration"].ToString());
                    sheet.Range[_row, 2, _row, 5].Merge();
                    _row++;

                    var _rowR = 5;

                    report.SetMasterHeaderText(ref sheet, _rowR, 3, "Voucher Date");
                    report.SetText(ref sheet, _rowR, 4, dtGeneralVoucher.Rows[0]["VoucherDate"].ToString());
                    sheet[report.GetColumnNameForXls(4) + _rowR + ":" + report.GetColumnNameForXls(5) + _rowR].Merge();
                    _rowR++;
                    report.SetMasterHeaderText(ref sheet, _rowR, 3, "Doc No");
                    report.SetText(ref sheet, _rowR, 4, dtGeneralVoucher.Rows[0]["DocRefNo"].ToString());
                    sheet[report.GetColumnNameForXls(4) + _rowR + ":" + report.GetColumnNameForXls(5) + _rowR].Merge();
                    _rowR++;
                    report.SetMasterHeaderText(ref sheet, _rowR, 3, "Fiscal Year");
                    report.SetText(ref sheet, _rowR, 4, dtGeneralVoucher.Rows[0]["FiscalYearName"] + " (" + dtGeneralVoucher.Rows[0]["PeriodNo"] + ")");
                    sheet[report.GetColumnNameForXls(4) + _rowR + ":" + report.GetColumnNameForXls(5) + _rowR].Merge();
                    _rowR++;
                    report.SetMasterHeaderText(ref sheet, _rowR, 3, "Status");
                    report.SetText(ref sheet, _rowR, 4, dtGeneralVoucher.Rows[0]["Park/Post"].ToString());
                    sheet[report.GetColumnNameForXls(4) + _rowR + ":" + report.GetColumnNameForXls(5) + _rowR].Merge();
                    _row++;
                    var _rowL = 11;
                    var headreColIndex = 1;

                    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 32);
                    headreColIndex++;
                    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 32);
                    headreColIndex++;
                    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 32);
                    headreColIndex++;
                    var sumdrcrCol = headreColIndex;

                    if (companyCurrencyId != tranCurrencyId)
                    {
                        report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, tranCurrencyCode, ExcelHAlign.HAlignCenter);
                        sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();

                        report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight);
                        headreColIndex++;

                        report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);
                        headreColIndex++;
                        lastColumn = 7;
                    }

                    report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
                    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight);
                    headreColIndex++;
                    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);

                    var plCurrencyId = string.Empty;
                    var plCurrencyCode = string.Empty;
                    shet2EndxlsCol = headreColIndex;

                    double tranAmount = 0;
                    double totalAmount = 0;
                    var Row_Total_Start = _rowL + 1;
                    var col = 1;

                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        _rowL++;
                        col = 1;
                        var Bank = dtMainBody.Rows[n]["BankMain"].ToString();
                        if (!string.IsNullOrEmpty(Bank))
                        {
                            report.SetText(ref sheet, _rowL, col, Bank); col++;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(dtMainBody.Rows[n]["PlantName"].ToString()))
                            {
                                report.SetText(ref sheet, _rowL, col, dtMainBody.Rows[n]["GLGeneralInfoCode"] + " - " + dtMainBody.Rows[n]["GL"] + " (" + dtMainBody.Rows[n]["PlantName"] + ")"); col++;
                            }
                            else
                            {
                                report.SetText(ref sheet, _rowL, col, dtMainBody.Rows[n]["GLGeneralInfoCode"] + " - " + dtMainBody.Rows[n]["GL"]); col++;
                            }
                        }
                        report.SetText(ref sheet, _rowL, col, dtMainBody.Rows[n]["Budget"].ToString()); col++;
                        report.SetText(ref sheet, _rowL, col, dtMainBody.Rows[n]["Activity"].ToString()); col++;
                        if (companyCurrencyId != tranCurrencyId)
                        {
                            report.SetText(ref sheet, _rowL, col, Convert.ToDouble(dtMainBody.Rows[n]["TDrAmount"])); col++;
                            report.SetText(ref sheet, _rowL, col, Convert.ToDouble(dtMainBody.Rows[n]["TCrAmount"])); col++;
                            tranAmount += Convert.ToDouble(dtMainBody.Rows[n]["TCrAmount"]);
                        }
                        report.SetText(ref sheet, _rowL, col, Convert.ToDouble(dtMainBody.Rows[n]["DrAmount"])); col++;
                        report.SetText(ref sheet, _rowL, col, Convert.ToDouble(dtMainBody.Rows[n]["CrAmount"]));
                        totalAmount += Convert.ToDouble(dtMainBody.Rows[n]["CrAmount"]);
                    }

                    _rowL++;
                    report.SetHeaderText(ref sheet, _rowL, 1, "Total :", ExcelHAlign.HAlignRight);
                    sheet.Range[_rowL, 1, _rowL, sumdrcrCol - 1].Merge();
                    if (companyCurrencyId != tranCurrencyId)
                    {
                        sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                        sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                        sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                        sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                        sumdrcrCol++;
                        sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                        sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                        sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                        sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                    }

                    sumdrcrCol++;
                    sheet.Range[_rowL, lastColumn - 1].Formula = "=SUM(" + report.GetColumnNameForXls(lastColumn - 1) + Row_Total_Start + ":" + report.GetColumnNameForXls(lastColumn - 1) + (_rowL - 1) + ")";
                    sheet.Range[_rowL, lastColumn - 1].NumberFormat = report.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, lastColumn - 1].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, lastColumn - 1].BorderAround(ExcelLineStyle.Hair);

                    sumdrcrCol++;
                    sheet.Range[_rowL, lastColumn].Formula = "=SUM(" + report.GetColumnNameForXls(lastColumn) + Row_Total_Start + ":" + report.GetColumnNameForXls(lastColumn) + (_rowL - 1) + ")";
                    sheet.Range[_rowL, lastColumn].NumberFormat = report.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, lastColumn].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, lastColumn].BorderAround(ExcelLineStyle.Hair);

                    var _Currency = string.Empty;
                    var _CurrencyId = string.Empty;

                    _Currency = dtGeneralVoucher.Rows[0]["TrnCurrency"].ToString();
                    _CurrencyId = dtGeneralVoucher.Rows[0]["CurrencyId"].ToString();

                    sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

                    _rowL++;
                    report.SetText(ref sheet, _rowL, 1, "In Word:", true);
                    if (companyCurrencyId != tranCurrencyId)
                    {
                        sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(tranAmount, tranCurrencyId);
                        sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                        sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                        sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;
                        _rowL++;
                    }

                    sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(totalAmount, companyCurrencyId);
                    sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                    sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;

                    sheet.UsedRange.AutofitColumns();
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                    _rowL = _rowL + 4;
                    report.SetSignatureText(ref sheet, _rowL - 1, 1, dtGeneralVoucher.Rows[0]["AddedBy"].ToString());
                    sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    report.SetTextMiddle(ref sheet, _rowL, 1, "Prepared By", true);

                    report.SetSignatureText(ref sheet, _rowL - 1, 3, dtGeneralVoucher.Rows[0]["PostedBy"].ToString());
                    sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    report.SetTextMiddle(ref sheet, _rowL, 3, "Checked By", true);

                    sheet.Range[_rowL, shet2EndxlsCol].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    report.SetTextMiddle(ref sheet, _rowL, lastColumn, "Authorized By", true);

                    report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, reportName, companyId, plantName, null);
                    report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                }
            }
            return workbook;
        }

        public IWorkbook GetAdvanceSetOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string reportName, SourceType sourceType)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";
            var advanceDataList = GetAdvanceSetOffData(companyGroupId, companyId, plantId, voucherId, sourceType);
            var dtGeneralVoucher = advanceDataList;
            var tranCurrencyId = dtGeneralVoucher.Rows[0]["CurrencyId"].ToString();
            var tranCurrencyCode = dtGeneralVoucher.Rows[0]["CurrencyCode"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var lastColumn = 5;
            using (var dvParallelCurrency = new DataView(advanceDataList))
            {
                var dtParallelCurrency = dvParallelCurrency.ToTable(true, "CurrencyCode", "ParallelCurrencyId");

                using (var dvCustomer = new DataView(advanceDataList)
                {
                    Sort = "Customer DESC"
                })
                {
                    var dtCustomer = dvCustomer.ToTable(true, "Customer");
                    var dvMainBody = new DataView(advanceDataList);
                    var dtMainBody = dvMainBody.ToTable(true, "VoucherDetailId", "Park/Post", "GLGeneralInfoCode", "PlantName", "GL", "Budget", "Activity", "TrnCurrency", "BankMain", "DrAmount", "CrAmount", "TDrAmount", "TCrAmount");
                    var _row = 5;
                    var shet2EndxlsCol = 1;

                    // Set report Name
                    reportFileName = Convert.ToDateTime(dtGeneralVoucher.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dtGeneralVoucher.Rows[0]["VoucherNo"];

                    report.SetMasterHeaderText(ref sheet, _row, 1, "Voucher No");
                    report.SetText(ref sheet, _row, 2, dtGeneralVoucher.Rows[0]["VoucherNo"].ToString());
                    _row++;
                    report.SetMasterHeaderText(ref sheet, _row, 1, "Doc Date");
                    report.SetText(ref sheet, _row, 2, dtGeneralVoucher.Rows[0]["DocDate"].ToString());
                    _row++;
                    report.SetMasterHeaderText(ref sheet, _row, 1, "Posting Date");
                    report.SetText(ref sheet, _row, 2, dtGeneralVoucher.Rows[0]["PostingDate"].ToString());
                    _row++;
                    report.SetMasterHeaderText(ref sheet, _row, 1, "Customer");
                    report.SetText(ref sheet, _row, 2, dtCustomer.Rows[0]["Customer"].ToString());
                    _row++;
                    report.SetMasterHeaderText(ref sheet, _row, 1, "Narration");
                    report.SetText(ref sheet, _row, 2, dtGeneralVoucher.Rows[0]["Narration"].ToString());
                    sheet.Range[_row, 2, _row, 5].Merge();
                    _row++;

                    var _rowR = 5;

                    report.SetMasterHeaderText(ref sheet, _rowR, 3, "Voucher Date");
                    report.SetText(ref sheet, _rowR, 4, dtGeneralVoucher.Rows[0]["VoucherDate"].ToString());
                    sheet[report.GetColumnNameForXls(4) + _rowR + ":" + report.GetColumnNameForXls(5) + _rowR].Merge();
                    _rowR++;
                    report.SetMasterHeaderText(ref sheet, _rowR, 3, "Doc No");
                    report.SetText(ref sheet, _rowR, 4, dtGeneralVoucher.Rows[0]["DocRefNo"].ToString());
                    sheet[report.GetColumnNameForXls(4) + _rowR + ":" + report.GetColumnNameForXls(5) + _rowR].Merge();
                    _rowR++;
                    report.SetMasterHeaderText(ref sheet, _rowR, 3, "Fiscal Year");
                    report.SetText(ref sheet, _rowR, 4, dtGeneralVoucher.Rows[0]["FiscalYearName"] + " (" + dtGeneralVoucher.Rows[0]["PeriodNo"] + ")");
                    sheet[report.GetColumnNameForXls(4) + _rowR + ":" + report.GetColumnNameForXls(5) + _rowR].Merge();
                    _rowR++;
                    report.SetMasterHeaderText(ref sheet, _rowR, 3, "Status");
                    report.SetText(ref sheet, _rowR, 4, dtGeneralVoucher.Rows[0]["Park/Post"].ToString());
                    sheet[report.GetColumnNameForXls(4) + _rowR + ":" + report.GetColumnNameForXls(5) + _rowR].Merge();
                    _row++;
                    var _rowL = 11;
                    var headreColIndex = 1;

                    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 32);
                    headreColIndex++;
                    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 32);
                    headreColIndex++;
                    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 32);
                    headreColIndex++;
                    var sumdrcrCol = headreColIndex;

                    if (companyCurrencyId != tranCurrencyId)
                    {
                        report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, tranCurrencyCode, ExcelHAlign.HAlignCenter);
                        sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();

                        report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight);
                        headreColIndex++;

                        report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);
                        headreColIndex++;
                        lastColumn = 7;
                    }

                    report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();
                    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight);
                    headreColIndex++;
                    report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);

                    var plCurrencyId = string.Empty;
                    var plCurrencyCode = string.Empty;
                    shet2EndxlsCol = headreColIndex;

                    double tranAmount = 0;
                    double totalAmount = 0;
                    var Row_Total_Start = _rowL + 1;
                    var col = 1;

                    for (int n = 0; n < dtMainBody.Rows.Count; n++)
                    {
                        _rowL++;
                        col = 1;
                        var Bank = dtMainBody.Rows[n]["BankMain"].ToString();
                        if (!string.IsNullOrEmpty(Bank))
                        {
                            report.SetText(ref sheet, _rowL, col, Bank); col++;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(dtMainBody.Rows[n]["PlantName"].ToString()))
                            {
                                report.SetText(ref sheet, _rowL, col, dtMainBody.Rows[n]["GLGeneralInfoCode"] + " - " + dtMainBody.Rows[n]["GL"] + " (" + dtMainBody.Rows[n]["PlantName"] + ")"); col++;
                            }
                            else
                            {
                                report.SetText(ref sheet, _rowL, col, dtMainBody.Rows[n]["GLGeneralInfoCode"] + " - " + dtMainBody.Rows[n]["GL"]); col++;
                            }
                        }
                        report.SetText(ref sheet, _rowL, col, dtMainBody.Rows[n]["Budget"].ToString()); col++;
                        report.SetText(ref sheet, _rowL, col, dtMainBody.Rows[n]["Activity"].ToString()); col++;
                        if (companyCurrencyId != tranCurrencyId)
                        {
                            report.SetText(ref sheet, _rowL, col, Convert.ToDouble(dtMainBody.Rows[n]["TDrAmount"])); col++;
                            report.SetText(ref sheet, _rowL, col, Convert.ToDouble(dtMainBody.Rows[n]["TCrAmount"])); col++;
                            tranAmount += Convert.ToDouble(dtMainBody.Rows[n]["TCrAmount"]);
                        }
                        report.SetText(ref sheet, _rowL, col, Convert.ToDouble(dtMainBody.Rows[n]["DrAmount"])); col++;
                        report.SetText(ref sheet, _rowL, col, Convert.ToDouble(dtMainBody.Rows[n]["CrAmount"]));
                        totalAmount += Convert.ToDouble(dtMainBody.Rows[n]["CrAmount"]);
                    }

                    _rowL++;
                    report.SetHeaderText(ref sheet, _rowL, 1, "Total :", ExcelHAlign.HAlignRight);
                    sheet.Range[_rowL, 1, _rowL, sumdrcrCol - 1].Merge();
                    if (companyCurrencyId != tranCurrencyId)
                    {
                        sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                        sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                        sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                        sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);

                        sumdrcrCol++;
                        sheet.Range[_rowL, sumdrcrCol].Formula = "=SUM(" + report.GetColumnNameForXls(sumdrcrCol) + Row_Total_Start + ":" + report.GetColumnNameForXls(sumdrcrCol) + (_rowL - 1) + ")";
                        sheet.Range[_rowL, sumdrcrCol].NumberFormat = report.NumberFormatDecimalTwo();
                        sheet.Range[_rowL, sumdrcrCol].CellStyle.Font.Bold = true;
                        sheet.Range[_rowL, sumdrcrCol].BorderAround(ExcelLineStyle.Hair);
                    }

                    sumdrcrCol++;
                    sheet.Range[_rowL, lastColumn - 1].Formula = "=SUM(" + report.GetColumnNameForXls(lastColumn - 1) + Row_Total_Start + ":" + report.GetColumnNameForXls(lastColumn - 1) + (_rowL - 1) + ")";
                    sheet.Range[_rowL, lastColumn - 1].NumberFormat = report.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, lastColumn - 1].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, lastColumn - 1].BorderAround(ExcelLineStyle.Hair);

                    sumdrcrCol++;
                    sheet.Range[_rowL, lastColumn].Formula = "=SUM(" + report.GetColumnNameForXls(lastColumn) + Row_Total_Start + ":" + report.GetColumnNameForXls(lastColumn) + (_rowL - 1) + ")";
                    sheet.Range[_rowL, lastColumn].NumberFormat = report.NumberFormatDecimalTwo();
                    sheet.Range[_rowL, lastColumn].CellStyle.Font.Bold = true;
                    sheet.Range[_rowL, lastColumn].BorderAround(ExcelLineStyle.Hair);

                    var _Currency = string.Empty;
                    var _CurrencyId = string.Empty;

                    _Currency = dtGeneralVoucher.Rows[0]["TrnCurrency"].ToString();
                    _CurrencyId = dtGeneralVoucher.Rows[0]["CurrencyId"].ToString();

                    sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[12, 1, _rowL, shet2EndxlsCol].BorderAround(ExcelLineStyle.Hair);

                    _rowL++;
                    report.SetText(ref sheet, _rowL, 1, "In Word:", true);
                    if (companyCurrencyId != tranCurrencyId)
                    {
                        sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(tranAmount, tranCurrencyId);
                        sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                        sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                        sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;
                        _rowL++;
                    }

                    sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(totalAmount, companyCurrencyId);
                    sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                    sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;

                    sheet.UsedRange.AutofitColumns();
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                    _rowL = _rowL + 4;
                    report.SetSignatureText(ref sheet, _rowL - 1, 1, dtGeneralVoucher.Rows[0]["AddedBy"].ToString());
                    sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    report.SetTextMiddle(ref sheet, _rowL, 1, "Prepared By", true);

                    report.SetSignatureText(ref sheet, _rowL - 1, 3, dtGeneralVoucher.Rows[0]["PostedBy"].ToString());
                    sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    report.SetTextMiddle(ref sheet, _rowL, 3, "Checked By", true);

                    sheet.Range[_rowL, shet2EndxlsCol].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                    report.SetTextMiddle(ref sheet, _rowL, lastColumn, "Authorized By", true);

                    report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, reportName, companyId, plantName, null);
                    report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                }
            }
            return workbook;
        }

        public string VendorAdvanceReport(string plantId, string companyGroupId, string companyId ,SourceType sourceType, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Vendor Advance Report";
                sheet = workbook.Worksheets[0];
                DataTable data;
                VendorAdvanceReportSQL(plantId,companyGroupId, companyId, sourceType, out data);

                int ROW = 6; int COL = 1;

                #region columns
                sheet[ROW, COL].Text = "Voucher No";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColVoucherNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Vendor";
                sheet[ROW, COL].ColumnWidth = 40;
                int ColVendor = COL;
                COL++;

                sheet[ROW, COL].Text = "Location";
                sheet[ROW, COL].ColumnWidth = 40;
                int ColLocation = COL;
                COL++;

                sheet[ROW, COL].Text = "Posting Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColPostingDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColCurrency = COL;
                COL++;

                sheet[ROW, COL].Text = "Advanced";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColAdvanced = COL;
                COL++;

                sheet[ROW, COL].Text = "Write-off";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColWriteoff = COL;
                COL++;

                sheet[ROW, COL].Text = "Balance";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColBalance = COL;
               

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, ColVendor].Text = data.Rows[i]["PartyName"].ToString();
                    sheet[ROW, ColLocation].Text = data.Rows[i]["PartyPlantName"].ToString();
                    sheet[ROW, ColPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
                    sheet[ROW, ColCurrency].Text = data.Rows[i]["CurrencyCode"].ToString();
                    sheet[ROW, ColAdvanced].Number = clsStaticInfo.dbl(data.Rows[i]["Receivable"].ToString());
                    sheet[ROW, ColWriteoff].Number = clsStaticInfo.dbl(data.Rows[i]["Received"].ToString());
                    sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Vendor Advance Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;




                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void VendorAdvanceReportSQL(string plantId, string companyGroupId, string companyId, SourceType sourceType, out DataTable data)
        {
            try
            {
                string strSQL = @"SELECT AD.AdvanceId, AD.Id AS AdvanceDetailId, AD.PartyType, AD.CompanyId, AD.PlantId, AM.PartyId, AM.PartyPlantId,P.Code AS  PartyCode, P.UserName As PartyName, PP.UserName AS PartyPlantName, AM.AdvanceNo, AM.VoucherId, VD.Id AS VoucherDetailId, VD.EntityId
								, EN.UserName AS EntityName, AM.CurrencyId, C.Code AS CurrencyCode, AD.GLGeneralInfoId AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
								, AD.BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, AD.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, V.VoucherNo, Replace(CONVERT(VARCHAR(11), AM.DocDate, 106), ' ', '-') AS DocDate
                                , Replace(CONVERT(VARCHAR(11), AM.PostingDate, 106), ' ', '-') AS PostingDate, AM.DocRefNo, AM.Narration, AD.Amount AS Receivable, AD.WrittenOffAmount AS Received
                                , AD.Amount-AD.WrittenOffAmount AS Balance, CC.CompanyCurrencyId, CC.CompanyFromCurrencyId, CC.ToCurrencyId, CC.CompanyCurrencyRate, CC.CompanyCurrencyConversion, GC.CompanyGroupCurrencyId
                                , GC.CompanyGroupFromCurrencyId, GC.CompanyGroupCurrencyRate, GC.CompanyGroupCurrencyConversion, HC.HardCurrencyId, HC.HardFromCurrencyId, HC.HardCurrencyRate, HC.HardCurrencyConversion
                                FROM [TRN].[AdvanceDetail] AS AD
                                LEFT JOIN [TRN].[Advance] AS AM ON AD.AdvanceId=AM.Id
                                LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdvanceDetailId=AD.Id
                                LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=AD.GLGeneralInfoId
                                LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=AD.BudgetMasterId
                                LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[Activity] AS A ON A.Id=AD.ActivityId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=AM.CurrencyId
                                LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=AM.EntityId
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=AM.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AM.PartyPlantId
								LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.CrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS CC ON CC.VoucherDetailId=VD.Id
							    LEFT JOIN (
							        SELECT VDC.ParallelCurrencyId AS CompanyGroupCurrencyId, VDC.FromCurrencyId AS CompanyGroupFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS CompanyGroupCurrencyRate, VDC.ToCurrencyConversion AS CompanyGroupCurrencyConversion, VDC.CrAmount AS CompanyGroupCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS GC ON GC.VoucherDetailId=VD.Id
							    LEFT JOIN (
								    SELECT VDC.ParallelCurrencyId AS HardCurrencyId, VDC.FromCurrencyId AS HardFromCurrencyId, VDC.ToCurrencyId,
								    VDC.ToCurrencyRate AS HardCurrencyRate, VDC.ToCurrencyConversion AS HardCurrencyConversion, VDC.CrAmount AS HardCurrencyAmount, VDC.VoucherDetailId
								    FROM [TRN].[VoucherDetailCurrency] AS VDC
								    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
								    WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"'
							    ) AS HC ON HC.VoucherDetailId=VD.Id
                                WHERE AM.Archive=0 AND AM.IsPosted=1 AND AM.IsWrittenOff=0 AND AD.IsWrittenOff=0 
                                AND AM.SourceType='" + sourceType + @"'
                                AND AM.CompanyGroupId='" + companyGroupId + "' AND AM.CompanyId='" + companyId + "' AND AM.PlantId='" + plantId + "'  ";

                data = _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public IWorkbook GetEmployeeAdvanceHRReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetEmployeeAdvanceHRReportHeader(companyGroupId, companyId, plantId, voucherId, sourceType);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetEmployeeAdvanceHRData(voucherId);
            var dsLocalARS = GetAdvanceReqSheduleData(voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colPurpose = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            sheet[row, 1].ColumnWidth = 20;
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            sheet[row, 2].ColumnWidth = 20;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Voucher Date");
            sheet[row, 6].ColumnWidth = 25;
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            reportUtility.SetText(ref sheet, row, 7, header["VoucherDate"].ToString());
            sheet[row, 7].ColumnWidth = 25;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "DocDate");
            reportUtility.SetText(ref sheet, row, 7, header["DocDate"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Employee");
            reportUtility.SetText(ref sheet, row, 2, header["EmployeeName"].ToString());
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 7, header["DocRefNo"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Status");
            reportUtility.SetText(ref sheet, row, 7, header["Status"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            colLast = companyCurrencyId == transcationCurrency ? 7 : 9;

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 6, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 8, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 8, row, 9].Merge();
            }
            sheet.Range[row, 6, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 6, row, colLast].BorderInside(ExcelLineStyle.Hair);

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++; /*xlsCol++;*/
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            //xlsCol++; //clo3


            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Purpose"); colPurpose = xlsCol;
            sheet[reportUtility.GetColumnNameForXls(colPurpose) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            xlsCol++;
            xlsCol++;
            xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 14, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 14, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
            }

            int formulaStartRow = 0;
            int formulaEndRow = 0;

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;

                formulaStartRow = row;

                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["Budget"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    reportUtility.SetText(ref sheet, row, colPurpose, dsLocal.Rows[i]["Purpose"].ToString());
                    sheet[row, 3].ColumnWidth = 25;
                    //sheet[row, colPurpose].WrapText = true;

                    sheet[reportUtility.GetColumnNameForXls(colPurpose) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
                    //xlsCol++;

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

                }
                formulaEndRow = row - 1;

                reportUtility.SetText(ref sheet, row, 5, "Total: ", true);
                sheet[row, 5].ColumnWidth = 20;
                sheet[row, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (row - 1) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (row - 1) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (row - 1) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (row - 1) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, colinrDebit, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, colinrDebit, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

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

                //sheet.UsedRange.AutofitColumns();
                //sheet[1, 2].ColumnWidth = 60;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet.Range[row, 1].ColumnWidth = 21;

                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Received By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 5, header["CheckedBy"].ToString());
                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Checked By", true);
                //sheet.Range[row, 5].ColumnWidth = 11;

                sheet.Range[row, 6].ColumnWidth = 21;

                reportUtility.SetSignatureText(ref sheet, row - 1, 7, header["ApprovedBy"].ToString());
                sheet.Range[row, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 7, "Approved By", true);
                sheet.Range[row, 7].ColumnWidth = 21;



                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }


            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            if (dsLocalARS.Rows.Count > 0)
            {
                row++;
                row++;
                row++;
                int COL = 1;
                int ROW = row;

                sheet[ROW, COL].Text = "Installment Date";
                int colInstallmentDate = COL;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet[ROW, COL].ColumnWidth = 20;
                COL++;

                sheet[ROW, COL].Text = "Installment No";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                int colInstallmentNo = COL;
                sheet[ROW, COL].ColumnWidth = 20;
                COL++;

                sheet[ROW, COL].Text = "Installment Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colInstallmentAmount = COL;
                sheet[ROW, COL].ColumnWidth = 17;
                COL++;

                sheet[ROW, COL].Text = "Profit Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colProfitAmount = COL;
                sheet[ROW, COL].ColumnWidth = 17;
                COL++;


                sheet[ROW, COL].Text = "Principal Amount";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPrincipalAmount = COL;
                sheet[ROW, COL].ColumnWidth = 17;
                COL++;

                sheet[ROW, COL].Text = "Balance";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colBalance = COL;
                sheet[ROW, COL].ColumnWidth = 17;
                COL++;

                sheet[ROW, COL].Text = "YearNo";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                int colYearNo = COL;
                sheet[ROW, COL].ColumnWidth = 10;
                COL++;

                sheet[ROW, COL].Text = "MonthNo";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                int colMonthNo = COL;
                sheet[ROW, COL].ColumnWidth = 10;
                COL++;

                sheet[ROW, COL].Text = "Status";
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                int colPaymentStatus = COL;
                sheet[ROW, COL].ColumnWidth = 8;
                //COL++;

                ROW++;

                int endCol = COL;

                int StartDataRow = ROW;

                for (int i = 0; i < dsLocalARS.Rows.Count; i++)
                {
                    sheet[ROW, colInstallmentDate].DateTime = Convert.ToDateTime(dsLocalARS.Rows[i]["InstallmentDate"].ToString());
                    sheet[ROW, colInstallmentDate].NumberFormat = "dd-MMM-yyyy";
                    sheet[ROW, colInstallmentDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet[ROW, colInstallmentNo].Text = dsLocalARS.Rows[i]["InstallmentNo"].ToString();
                    sheet[ROW, colInstallmentNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet[ROW, colInstallmentAmount].Number = clsStaticInfo.dbl(dsLocalARS.Rows[i]["InstallmentAmount"].ToString());
                    sheet[ROW, colInstallmentAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet[ROW, colProfitAmount].Number = clsStaticInfo.dbl(dsLocalARS.Rows[i]["ProfitAmount"].ToString());
                    sheet[ROW, colProfitAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet[ROW, colPrincipalAmount].Number = clsStaticInfo.dbl(dsLocalARS.Rows[i]["PrincipalAmount"].ToString());
                    sheet[ROW, colPrincipalAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                    sheet[ROW, colBalance].Number = clsStaticInfo.dbl(dsLocalARS.Rows[i]["Balance"].ToString());
                    sheet[ROW, colBalance].NumberFormat = "#,##0.00;(#,##0.00)";

                    sheet[ROW, colYearNo].Text = dsLocalARS.Rows[i]["YearNo"].ToString();
                    sheet[ROW, colYearNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet[ROW, colMonthNo].Text = dsLocalARS.Rows[i]["MonthNo"].ToString();
                    //sheet[ROW, colMonthNo].Text = dsLocalARS.Rows[i]["MonthNo"].ToString();
                    sheet[ROW, colMonthNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet[ROW, colPaymentStatus].Text = dsLocalARS.Rows[i]["Status"].ToString();
                    //sheet[ROW, colMonthNo].Text = dsLocalARS.Rows[i]["MonthNo"].ToString();
                    sheet[ROW, colPaymentStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;


                    ROW++;
                }


                sheet[StartDataRow - 1, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet[StartDataRow - 1, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);
            }


            return workbook;
        }

        private Dictionary<string, object> GetEmployeeAdvanceHRReportHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, U.FullName AddedBy,isnull(PC.EmployeeName,UP.FullName) CheckedBy,PA.EmployeeName ApprovedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            ,EmployeeName=P.EmployeeName, ESA.CurrencyId, C.Code AS CurrencyCode,EAR.Remarks Purpose
                            FROM [TRN].[Voucher] AS V
                            LEFT JOIN [TRN].[EmployeeAdvanceDetail] AS BJ  ON V.Id=BJ.VoucherId
                            LEFT JOIN TRN.EmployeeAdvance AS ESA  ON  ESA.Id=BJ.EmployeeAdvanceId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [DBO].[EmployeeInformation] AS P ON P.SystemId=BJ.EmpSystemId
							left join TRN.EmployeeAdvanceRequisition EAR ON EAR.SystemId=ESA.RequisitionId
							LEFT JOIN [DBO].[EmployeeInformation] AS PA ON PA.SystemId=EAR.ApprovedBy
							 LEFT JOIN [DBO].[EmployeeInformation] AS PC ON PC.SystemId=EAR.CheckedBy
							 LEFT JOIN [SEC].[User] AS U ON U.UserId=V.AddedBy
							LEFT JOIN [SEC].[User] AS UP ON UP.UserId=V.PostedBy
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        private DataTable GetEmployeeAdvanceHRData(string voucherId)
        {
            try
            {
                var sql = @"SELECT V.Id, GL.Id AS AccountCodeId, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.VoucherNo, UPPER(V.Narration) AS Narration
                            , V.CurrencyId, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, CU1.Code AS TrnCurrency, U.FullName AddedBy,isnull(PC.EmployeeName,UP.FullName) CheckedBy ,  VDC.ParallelCurrencyId, CU.Code AS CurrencyCode
                            , VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate, VD.DrAmount AS DrAmount, VD.CrAmount AS CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END
                            , VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode, P.EmployeeName , VD.Narration AS DetailNarration, BUD.UserName AS Budget
							,EAR.Remarks Purpose,PA.EmployeeName ApprovedBy
	                        ,Activity = case when vd.CashMasterId<>'' then cm.UserName when VD.BankMasterId<>'' then BM.AccountTitle else ACT.UserName end

                            , CM.UserName AS CashMasterName
                            FROM [TRN].[VoucherDetailCurrency] AS VDC
                            JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=VDC.VoucherDetailId
                            JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                            LEFT JOIN [TRN].[AdvanceDetail] AS IVD ON IVD.Id=VD.AdvanceDetailId
                            LEFT JOIN [TRN].[Advance] AS IV ON IV.Id=IVD.AdvanceId
                            LEFT JOIN [DBO].[EmployeeInformation] AS P ON P.SystemId=IV.EmployeeId
							left join TRN.EmployeeAdvanceRequisition EAR ON EAR.SystemId=IV.RequisitionId
							 LEFT JOIN [DBO].[EmployeeInformation] AS PA ON PA.SystemId=EAR.ApprovedBy
							  LEFT JOIN [DBO].[EmployeeInformation] AS PC ON PC.SystemId=EAR.CheckedBy
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                            LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                            LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                            LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                            LEFT JOIN [MST].[BudgetMaster] BUM ON VD.BudgetMasterId=BUM.Id
                            LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUM.BudgetId
                            LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            left join [MST].[BankMaster] BM on BM.Id=VD.BankMasterId
							LEFT JOIN [SEC].[User] AS U ON U.UserId=V.AddedBy
							LEFT JOIN [SEC].[User] AS UP ON UP.UserId=V.PostedBy
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