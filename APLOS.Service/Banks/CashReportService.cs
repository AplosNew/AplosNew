using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Currencies;
using Library.Service.Extension.Accounts;
using Library.Service.Helpers;
using Library.Service.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Data;
using System.Linq;


namespace Library.Service.Banks
{
    public class CashReportService : ICashReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly ICashJournalService _cashJournalService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;

        public CashReportService(
              ISqlRepository sqlRepository
            , ICashJournalService cashJournalService
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService
            )
        {
            _sqlRepository = sqlRepository;
            _cashJournalService = cashJournalService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
        }

        private IWorkbook xGetCashJVReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = _cashJournalService.GetCashJournalHeader(companyGroupId, companyId, plantId, voucherId, sourceType);
            // Set report Name
            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = _cashJournalService.GetCashJournalDetail(companyGroupId, companyId, plantId, voucherId, sourceType);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Date");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());

            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash");
            reportUtility.SetText(ref sheet, row, 2, header["CashName"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
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
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, 1, "GL");
            reportUtility.SetHeaderText(ref sheet, row, 2, "", 36);
            reportUtility.SetHeaderText(ref sheet, row, 3, "Particulars", 14);
            sheet[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Credit", 13, ExcelHAlign.HAlignRight);

                reportUtility.SetHeaderText(ref sheet, row, 6, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 7, "Credit", 13, ExcelHAlign.HAlignRight);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Credit", 13, ExcelHAlign.HAlignRight);
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BankName"].ToString();
                    if (string.IsNullOrEmpty(glName))
                        glName = dsLocal.Rows[i]["CashName"].ToString();
                    if (string.IsNullOrEmpty(glName))
                        glName = dsLocal.Rows[i]["AssetUserName"].ToString();
                    if (string.IsNullOrEmpty(glName))
                        glName = dsLocal.Rows[i]["ExpensesUserName"].ToString();
                    if (string.IsNullOrEmpty(glName))
                        glName = dsLocal.Rows[i]["ActivityName"].ToString();

                    reportUtility.SetText(ref sheet, row, 1, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["ActivityName"]);
                    sheet[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();

                    var partyName = dsLocal.Rows[i]["PartyName"].ToString();
                    if (string.IsNullOrEmpty(partyName))
                        partyName = dsLocal.Rows[i]["EmployeeName"].ToString();
                    reportUtility.SetText(ref sheet, row, 3, partyName);

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, 4, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, 4, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());
                    row++;
                    glName = string.Empty;
                }

                if (companyCurrencyId != transcationCurrency)
                {
                    reportUtility.SetText(ref sheet, row, colLast - 4, "Total: ", true);
                }
                else
                {
                    reportUtility.SetText(ref sheet, row, colLast - 2, "Total: ", true);
                }

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, 4].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 11 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, 4].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 4].CellStyle.Font.Bold = true;
                    sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 4].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 5].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 11 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 5].CellStyle.Font.Bold = true;
                    sheet.Range[row, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 5].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 6].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 11 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 6].CellStyle.Font.Bold = true;
                    sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 6].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 6].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + 11 + ":" + reportUtility.GetColumnNameForXls(7) + (row - 1) + ")";
                    sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 7].CellStyle.Font.Bold = true;
                    sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 7].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 7].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, 4].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 11 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, 4].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 4].CellStyle.Font.Bold = true;
                    sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 4].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 5].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 11 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 5].CellStyle.Font.Bold = true;
                    sheet.Range[row, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 5].BorderAround(ExcelLineStyle.Hair);
                }
                sheet.Range[11, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[11, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 1;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);
                if (companyCurrencyId != transcationCurrency)
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
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetText(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetText(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetText(ref sheet, row, colLast, "Authorized By", true);

                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        //public IWorkbook GetCashJVReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        //{
        //    var reportUtility = new ReportUtility();
        //    var excelEngine = new ExcelEngine();
        //    var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
        //    workbook.Version = ExcelVersion.Excel2016;
        //    var sheet = workbook.Worksheets[0];
        //    sheet.Name = "Voucher";
        //    var header = _cashJournalService.GetCashJournalHeader(companyGroupId, companyId, plantId, voucherId, sourceType);


        //    reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];
        //    var dsLocal = _cashJournalService.GetCashJournalDetail(companyGroupId, companyId, plantId, voucherId, sourceType);


        //    var transcationCurrency = header["CurrencyId"].ToString();
        //    _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

        //    var row = 5;

        //    var colLast = 1;

        //    int xlsCol = 1;
        //    int colGl = 0;
        //    int colParticulars = 0;
        //    int colinrDebit = 0;
        //    int colinrCredit = 0;
        //    int colusdDebit = 0;
        //    int colusdCradit = 0;

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
        //    reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
        //    reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Entry Date");
        //    reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

        //    sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

        //    row++;

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
        //    reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
        //    reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
        //    reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

        //    sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

        //    row++;

        //    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
        //    reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
        //    reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
        //    reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

        //    sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

        //    row++;

        //    colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
        //    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
        //    reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
        //    sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

        //    row++;

        //    if (companyCurrencyId == transcationCurrency)
        //    {
        //        reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
        //        sheet[row, 4, row, 5].Merge();
        //        sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
        //    }
        //    else
        //    {
        //        reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
        //        sheet[row, 4, row, 5].Merge();

        //        reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
        //        sheet[row, 6, row, 7].Merge();
        //        sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
        //    }

        //    row++;

        //    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
        //    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
        //    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
        //    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

        //    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

        //    if (companyCurrencyId != transcationCurrency)
        //    {
        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
        //        colLast = xlsCol;
        //    }
        //    else
        //    {
        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
        //        reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
        //        colLast = xlsCol;
        //    }

        //    if (dsLocal.Rows.Count > 0)
        //    {
        //        double totalTranAmount = 0;
        //        double totalBookCurrencyAmount = 0;
        //        var xRow = row;
        //        row++;
        //        for (int i = 0; i < dsLocal.Rows.Count; i++)
        //        {
        //            var glName = dsLocal.Rows[i]["BudgetName"].ToString();


        //            reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

        //            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


        //            reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

        //            if (companyCurrencyId != transcationCurrency)
        //            {
        //                reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
        //                reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
        //                reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
        //                reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
        //                totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
        //            }
        //            else
        //            {
        //                reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
        //                reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
        //            }
        //            totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

        //            sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
        //            sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
        //            row++;

        //            glName = string.Empty;

        //        }


        //        reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
        //        var lastRow = row - 1;

        //        if (companyCurrencyId != transcationCurrency)
        //        {
        //            sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
        //            sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

        //            sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
        //            sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

        //            sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
        //            sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

        //            sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
        //            sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
        //        }
        //        else
        //        {
        //            sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
        //            sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

        //            sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
        //            sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
        //            sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
        //            sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
        //            sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
        //            sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
        //        }

        //        row += 2;
        //        reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

        //        if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
        //        {
        //            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
        //            sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
        //            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
        //            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
        //            row++;
        //        }

        //        sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
        //        sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
        //        sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //        sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
        //        sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

        //        sheet.UsedRange.AutofitColumns();
        //        sheet[1, 2].ColumnWidth = 40;
        //        sheet.UsedRange.CellStyle.Font.Size = 8;
        //        row += 4;
        //        reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
        //        sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //        reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

        //        reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
        //        sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //        reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

        //        sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //        reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

        //        reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
        //        reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
        //    }
        //    else
        //    {
        //        sheet.UsedRange.WrapText = true;
        //        sheet.UsedRange.CellStyle.Font.Size = 8;
        //        reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
        //        reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
        //    }
        //    return workbook;
        //}

        public IWorkbook GetCashJVReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Cash Journal";

            // var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.AdvanceJournalVoucher);
            //var header = _bankJournalService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            BankExtensionService bankExtensionService = new BankExtensionService();

            var header = bankExtensionService.GetCashJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.CashJournal);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            //  var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);
            var dsLocal = bankExtensionService.GetCashJournalDetail(companyGroupId, companyId, plantId, voucherId, SourceType.CashJournal);

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


            //int colCheckNo = colVoucherNo;
            //int colCheckNoValue = colVoucherNoValue;
            //reportUtility.SetMasterHeaderText(ref sheet, row, colCheckNo, "CheckNo");
            //reportUtility.SetText(ref sheet, row, colCheckNoValue, header["CheckNumber"].ToString());


            //int colCheckDate = colVoucherDate;
            //int colCheckDateValue = colVoucherDateValue;
            //reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            //reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());
            //row++;

            //int colParty = colVoucherNo;
            //int colPartyValue = colVoucherNoValue;
            //reportUtility.SetMasterHeaderText(ref sheet, row, colParty, "Party");
            //reportUtility.SetText(ref sheet, row, colPartyValue, header["Party"].ToString());



            int colNaration = colVoucherNo;
            reportUtility.SetMasterHeaderText(ref sheet, row, colNaration, "Narration");
            int colNarationValue = colVoucherNoValue;
            reportUtility.SetText(ref sheet, row, colNarationValue, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

            sheet.Range[row, colNaration].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colNarationValue].VerticalAlignment = ExcelVAlign.VAlignTop;



            int colDocRef = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colDocRef, "Doc Ref");
            sheet.Range[row, colDocRef].VerticalAlignment = ExcelVAlign.VAlignTop;

            int colDocRefValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colDocRefValue, header["DocRefNo"].ToString());
            sheet.Range[row, colDocRefValue].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            //int colNaration = colVoucherNo;
            //reportUtility.SetMasterHeaderText(ref sheet, row, colNaration, "Narration");
            //int colNarationValue = colVoucherNoValue;
            //reportUtility.SetText(ref sheet, row, colNarationValue, header["Narration"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(colVoucherNoValue) + row + ":" + reportUtility.GetColumnNameForXls(colParticulars) + row].Merge();

            //sheet.Range[row, colNaration].VerticalAlignment = ExcelVAlign.VAlignTop;
            //sheet.Range[row, colNarationValue].VerticalAlignment = ExcelVAlign.VAlignTop;


            int colStatus = colVoucherDate;
            reportUtility.SetMasterHeaderText(ref sheet, row, colStatus, "Status");
            int colStatusValue = colVoucherDateValue;
            reportUtility.SetText(ref sheet, row, colStatusValue, header["Status"].ToString());
            sheet.Range[row, colStatus].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, colStatusValue].VerticalAlignment = ExcelVAlign.VAlignTop;
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

                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();

                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["ActivityName"]);

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


                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }



        public IWorkbook GetCashJournalReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = _cashJournalService.GetCashJournalHeader(companyGroupId, companyId, plantId, voucherId, sourceType);
            // Set report Name
            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = _cashJournalService.GetCashJournalDetail(companyGroupId, companyId, plantId, voucherId, sourceType);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Doc Date");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());

            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash");
            reportUtility.SetText(ref sheet, row, 2, header["CashName"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
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
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, 1, "GL");
            reportUtility.SetHeaderText(ref sheet, row, 2, "", 22);
            reportUtility.SetHeaderText(ref sheet, row, 3, "", 14);
            sheet[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Credit", 13, ExcelHAlign.HAlignRight);

                reportUtility.SetHeaderText(ref sheet, row, 6, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 7, "Credit", 13, ExcelHAlign.HAlignRight);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, "Debit", 13, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Credit", 13, ExcelHAlign.HAlignRight);
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BankName"].ToString();
                    if (string.IsNullOrEmpty(glName))
                        glName = dsLocal.Rows[i]["CashName"].ToString();
                    if (string.IsNullOrEmpty(glName))
                        glName = dsLocal.Rows[i]["AssetUserName"].ToString();
                    if (string.IsNullOrEmpty(glName))
                        glName = dsLocal.Rows[i]["ExpensesUserName"].ToString();
                    if (string.IsNullOrEmpty(glName))
                        glName = dsLocal.Rows[i]["ActivityName"].ToString();

                    reportUtility.SetText(ref sheet, row, 1, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName);
                    sheet[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, 4, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, 4, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());
                    row++;
                    glName = string.Empty;
                }

                if (companyCurrencyId != transcationCurrency)
                {
                    reportUtility.SetText(ref sheet, row, colLast - 4, "Total: ", true);
                }
                else
                {
                    reportUtility.SetText(ref sheet, row, colLast - 2, "Total: ", true);
                }

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, 4].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 11 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, 4].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 4].CellStyle.Font.Bold = true;
                    sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 4].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 5].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 11 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 5].CellStyle.Font.Bold = true;
                    sheet.Range[row, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 5].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 6].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 11 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 6].CellStyle.Font.Bold = true;
                    sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 6].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 6].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + 11 + ":" + reportUtility.GetColumnNameForXls(7) + (row - 1) + ")";
                    sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 7].CellStyle.Font.Bold = true;
                    sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 7].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 7].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, 4].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 11 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, 4].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 4].CellStyle.Font.Bold = true;
                    sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 4].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 5].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 11 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 5].CellStyle.Font.Bold = true;
                    sheet.Range[row, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 5].BorderAround(ExcelLineStyle.Hair);
                }
                sheet.Range[11, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[11, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 1;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);
                if (companyCurrencyId != transcationCurrency)
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
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetText(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetText(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetText(ref sheet, row, colLast, "Authorized By", true);

                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }

        public IWorkbook GetCashOpeningBalanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId, bool isCompanyCurrency)
        {
            try
            {
                var row = 5;
                var colLast = 1;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Cash Opening Balance";

                // Get bank transaction data.
                var fiscalYear = _sqlRepository.GetData("SELECT FiscalYearCode, FiscalYearName, StartDate, EndDate FROM [SCS].[FiscalYear] WHERE Id='" + fiscalYearId + "'");
                var ledgerData = _cashJournalService.GetCashLedgerData(companyGroupId, companyId, plantId, null, null, null, true, fiscalYearId);
                if (ledgerData.Rows.Count > 0)
                {
                    // Set Header Column
                    row++;
                    reportUtility.SetHeaderText(ref sheet, row, 6, "Cash Currency", ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(6) + row + ":" + reportUtility.GetColumnNameForXls(7) + row].Merge();

                    colLast = 6;
                    _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                    if (isCompanyCurrency && !string.IsNullOrEmpty(companyCurrencyId))
                    {
                        reportUtility.SetHeaderText(ref sheet, row, 8, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                        sheet.Range[reportUtility.GetColumnNameForXls(8) + row + ":" + reportUtility.GetColumnNameForXls(9) + row].Merge();
                        colLast = 9;
                    }

                    // Detail Header
                    row++;
                    reportUtility.SetHeaderText(ref sheet, row, 1, "Cash", 26);
                    reportUtility.SetHeaderText(ref sheet, row, 2, "Voucher No", 12);
                    reportUtility.SetHeaderText(ref sheet, row, 3, "Posting Date", 10);
                    reportUtility.SetHeaderText(ref sheet, row, 4, "Narration", 12);
                    reportUtility.SetHeaderText(ref sheet, row, 5, "Cash Currency", 12);
                    reportUtility.SetHeaderText(ref sheet, row, 6, "Debit", 10, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 7, "Credit", 10, ExcelHAlign.HAlignRight);

                    if (isCompanyCurrency && !string.IsNullOrEmpty(companyCurrencyId))
                    {
                        reportUtility.SetHeaderText(ref sheet, row, 8, "Debit", 10, ExcelHAlign.HAlignRight);
                        reportUtility.SetHeaderText(ref sheet, row, 9, "Credit", 10, ExcelHAlign.HAlignRight);
                    }

                    row++;
                    if (ledgerData.Rows.Count > 0)
                    {
                        for (int i = 0; i < ledgerData.Rows.Count; i++)
                        {
                            reportUtility.SetText(ref sheet, row, 1, ledgerData.Rows[i]["CashName"].ToString());
                            reportUtility.SetText(ref sheet, row, 2, ledgerData.Rows[i]["VoucherNo"].ToString());
                            reportUtility.SetText(ref sheet, row, 3, ledgerData.Rows[i]["PostingDate"].ToString());
                            reportUtility.SetText(ref sheet, row, 4, ledgerData.Rows[i]["Narration"].ToString());
                            reportUtility.SetText(ref sheet, row, 5, ledgerData.Rows[i]["CurrencyCode"].ToString());
                            reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString()));

                            // Base currency checking
                            if (isCompanyCurrency && !string.IsNullOrEmpty(companyCurrencyId))
                            {
                                reportUtility.SetText(ref sheet, row, 8, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, 9, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            }
                            row++;
                        }
                    }
                    sheet.Range[8, 1, (row - 1), 9].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[8, 1, (row - 1), 9].BorderAround(ExcelLineStyle.Hair);
                    sheet.UsedRange.WrapText = false;
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                }
                else
                {
                    reportUtility.SetText(ref sheet, 6, colLast, "Data not found!", ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(1) + 6 + ":" + reportUtility.GetColumnNameForXls(colLast) + 6].Merge();
                }
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Cash Opening Balance Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "Fiscal Year : " + fiscalYear["FiscalYearName"], ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetCashLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string cashMasterId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                // Get BankMaster data
                var cashMaster = _cashJournalService.GetCashMaster(cashMasterId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, cashMaster["CashName"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, cashMaster["GLGeneralInfoCode"] + " - " + cashMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, cashMaster["CurrencyCode"].ToString());

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 4, "Cash Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(7) + row].Merge();

                colLast = 7;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                var cashCurrencyId = cashMaster["CurrencyId"].ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(6) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();
                    colLast = 9;
                }

                // Detail Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, 1, "Voucher No", 12);
                reportUtility.SetHeaderText(ref sheet, row, 2, "Account Name", 12);
                reportUtility.SetHeaderText(ref sheet, row, 3, "Narration", 32);
                reportUtility.SetHeaderText(ref sheet, row, 4, "Debit", 9, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Credit", 9, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 6, "Balance", 12, ExcelHAlign.HAlignRight);
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 7, "Debit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 8, "Credit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 9, "Balance", ExcelHAlign.HAlignRight);
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);
                row++;

                // Get Cash transaction data.
                var ledgerData = _cashJournalService.GetCashLedgerData(companyGroupId, companyId, plantId, cashMasterId, fromDate, toDate);
                var obVal = _cashJournalService.GetCashOpeningBalanceLedgerData(companyGroupId, companyId, plantId, cashMasterId, fromDate);
                if (ledgerData.Rows.Count > 0)
                {
                    var dt = ledgerData.AsEnumerable().OrderBy(r => Convert.ToDateTime(r["PostingDate"]))
                            .GroupBy(r => new { PostingDate = r["PostingDate"] })
                            .Select(g => g.OrderBy(r => r["PostingDate"]).First())
                            .CopyToDataTable();
                    var isOB = true;
                    var lastClosing = string.Empty; ;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        var data = ledgerData.AsEnumerable()
                            .Where(r => r.Field<string>("PostingDate") == dt.Rows[j]["PostingDate"].ToString())
                            .OrderBy(r => r["VoucherNo"])
                            .CopyToDataTable();

                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                        reportUtility.SetText(ref sheet, row, 1, "As On " + dt.Rows[j]["PostingDate"]);
                        sheet.Range[row, 1].CellStyle.Font.Bold = true;
                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].BorderAround(ExcelLineStyle.Hair);
                        row++;
                        
                        reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();
                        // Get Cash opening balance data.
                        if (obVal.Count > 0 && isOB)
                        {
                            // Set Opening Balance
                            var ob = Convert.ToDouble(obVal[0]["OB"]);
                            reportUtility.SetText(ref sheet, row, 6, ob, true);
                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                                reportUtility.SetText(ref sheet, row, 8, ob, true);
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            isOB = false;
                        }
                        else
                        {
                            reportUtility.SetFormula(ref sheet, row, 6, lastClosing, true);
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        }

                        row++;
                        for (int i = 0; i < data.Rows.Count; i++)
                        {
                            reportUtility.SetText(ref sheet, row, 1, data.Rows[i]["VoucherNo"].ToString());
                            reportUtility.SetText(ref sheet, row, 2, data.Rows[i]["OtherSide"].ToString());
                            reportUtility.SetText(ref sheet, row, 3, data.Rows[i]["Narration"].ToString());
                            reportUtility.SetText(ref sheet, row, 4, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, 6].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(4) + row + "-" + reportUtility.GetColumnNameForXls(5) + row + ")";
                            sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;

                            // Base currency checking
                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, 8, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                                sheet.Range[row, 9].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(9) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(7) + row + "-" + reportUtility.GetColumnNameForXls(8) + row + ")";
                                sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                sheet.Range[row, 9].VerticalAlignment = ExcelVAlign.VAlignTop;
                            }
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            row++;
                        }
                        reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

                        sheet.Range[row, 6].Formula = "=" + reportUtility.GetColumnNameForXls(6) + (row - 1);
                        lastClosing = "=" + reportUtility.GetColumnNameForXls(6) + (row - 1);
                        sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, 6].CellStyle.Font.Bold = true;
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            sheet.Range[row, 9].Formula = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                            sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 9].CellStyle.Font.Bold = true;
                        }
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }
                }

                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Cash Book", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook xGetCashBookReport(string companyGroupId, string companyId, string plantId, string plantName, string cashMasterId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Report";

                // Get BankMaster data
                var cashMaster = _cashJournalService.GetCashMaster(cashMasterId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, cashMaster["CashName"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, cashMaster["GLGeneralInfoCode"] + " - " + cashMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, cashMaster["CurrencyCode"].ToString());

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 5, "Cash Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();

                colLast = 8;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                var cashCurrencyId = cashMaster["CurrencyId"].ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 7, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(7) + row + ":" + reportUtility.GetColumnNameForXls(9) + row].Merge();
                    colLast = 10;
                }

                // Detail Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, 1, "Voucher No", 12);
                reportUtility.SetHeaderText(ref sheet, row, 2, "Posting Date", 10);
                reportUtility.SetHeaderText(ref sheet, row, 3, "Account Name", 12);
                reportUtility.SetHeaderText(ref sheet, row, 4, "Narration", 32);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Debit", 9, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 6, "Credit", 9, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 7, "Balance", 12, ExcelHAlign.HAlignRight);
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 8, "Debit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 9, "Credit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 10, "Balance", ExcelHAlign.HAlignRight);
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

                // Get Cash opening balance data.
                var obVal = _cashJournalService.GetCashOpeningBalanceLedgerData(companyGroupId, companyId, plantId, cashMasterId, fromDate);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    var ob = Convert.ToDouble(obVal[0]["OB"]);
                    reportUtility.SetText(ref sheet, row, 7, ob, true);

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        reportUtility.SetText(ref sheet, row, 10, ob, true);
                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                }

                row++;
                // Get Cash transaction data.
                var ledgerData = _cashJournalService.GetCashLedgerData(companyGroupId, companyId, plantId, cashMasterId, fromDate, toDate);
                if (ledgerData.Rows.Count > 0)
                {
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        reportUtility.SetText(ref sheet, row, 1, ledgerData.Rows[i]["VoucherNo"].ToString());
                        reportUtility.SetText(ref sheet, row, 2, ledgerData.Rows[i]["PostingDate"].ToString());
                        reportUtility.SetText(ref sheet, row, 3, ledgerData.Rows[i]["OtherSide"].ToString());
                        reportUtility.SetText(ref sheet, row, 4, ledgerData.Rows[i]["Narration"].ToString());

                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString()));
                            sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(5) + row + "-" + reportUtility.GetColumnNameForXls(6) + row + ")";
                            sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;

                            reportUtility.SetText(ref sheet, row, 8, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 9, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, 10].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(10) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(8) + row + "-" + reportUtility.GetColumnNameForXls(9) + row + ")";
                            sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 10].VerticalAlignment = ExcelVAlign.VAlignTop;
                        }
                        else
                        {
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(5) + row + "-" + reportUtility.GetColumnNameForXls(6) + row + ")";
                            sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                        }
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

                sheet.Range[row, 7].Formula = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, 7].CellStyle.Font.Bold = true;
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    sheet.Range[row, 10].Formula = "=" + reportUtility.GetColumnNameForXls(10) + (row - 1);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 10].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";

                sheet.Range[11, 4, row, 4].WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Cash Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IWorkbook GetCashBookReport(string companyGroupId, string companyId, string plantId, string plantName, string cashMasterId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Report";

                // Get BankMaster data
                var cashMaster = _cashJournalService.GetCashMaster(cashMasterId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, cashMaster["CashName"].ToString());
                sheet[row, 3].ColumnWidth = 150;

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, cashMaster["GLGeneralInfoCode"] + " - " + cashMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();


                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, cashMaster["CurrencyCode"].ToString());

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 5, "Cash Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();
                reportUtility.SetHeaderText(ref sheet, row, 6, "Cash Currency", ExcelHAlign.HAlignCenter, true);
                reportUtility.SetHeaderText(ref sheet, row, 7, "Cash Currency", ExcelHAlign.HAlignCenter, true);
                reportUtility.SetHeaderText(ref sheet, row, 8, "Cash Currency", ExcelHAlign.HAlignCenter, true);
                colLast = 8;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                var cashCurrencyId = cashMaster["CurrencyId"].ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 7, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(7) + row + ":" + reportUtility.GetColumnNameForXls(9) + row].Merge();
                    colLast = 10;
                }

                // Detail Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, 1, "Voucher No", 12);
                reportUtility.SetHeaderText(ref sheet, row, 2, "Posting Date", 12);
                reportUtility.SetHeaderText(ref sheet, row, 3, "Particulars", 12);
                reportUtility.SetHeaderText(ref sheet, row, 4, "Narration", 32);

                //sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ": " + reportUtility.GetColumnNameForXls(4) + row].Merge();
                reportUtility.SetHeaderText(ref sheet, row, 5, "Debit", 9, ExcelHAlign.HAlignRight); int colDebit = 5;
                reportUtility.SetHeaderText(ref sheet, row, 6, "Credit", 9, ExcelHAlign.HAlignRight); int colCredit = 6;
                reportUtility.SetHeaderText(ref sheet, row, 7, "Balance", 12, ExcelHAlign.HAlignRight);
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 8, "Debit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 9, "Credit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 10, "Balance", ExcelHAlign.HAlignRight);
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);

                row++;
                reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

                // Get Cash opening balance data.
                var obVal = _cashJournalService.GetCashOpeningBalanceLedgerData(companyGroupId, companyId, plantId, cashMasterId, fromDate);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    var ob = Convert.ToDouble(obVal[0]["OB"]);
                    reportUtility.SetText(ref sheet, row, 7, ob, true);

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        reportUtility.SetText(ref sheet, row, 10, ob, true);
                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                }

                row++;
                int StartRow = row;
                // Get Cash transaction data.
                //var ledgerData = _cashJournalService.GetCashLedgerData(companyGroupId, companyId, plantId, cashMasterId, fromDate, toDate);

                var ledgerData = GetCashLedgerDataByplant(companyGroupId, companyId, plantId, cashMasterId, fromDate, toDate);
                if (ledgerData.Rows.Count > 0)
                {
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        reportUtility.SetText(ref sheet, row, 1, ledgerData.Rows[i]["VoucherNo"].ToString());
                        reportUtility.SetText(ref sheet, row, 2, Convert.ToDateTime(ledgerData.Rows[i]["PostingDate"].ToString()).ToString("dd-MMM-yyyy"));
                        reportUtility.SetText(ref sheet, row, 3, ledgerData.Rows[i]["OtherSide"].ToString());
                        reportUtility.SetText(ref sheet, row, 4, ledgerData.Rows[i]["Narration"].ToString());
                        //sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ": " + reportUtility.GetColumnNameForXls(4) + row].Merge();
                        //sheet.Range[row, 3,row,4].WrapText = true;
                        reportUtility.SetText(ref sheet, row, 4, ledgerData.Rows[i]["Narration"].ToString(), false, true);
                        sheet.Range[row, 3].WrapText = true;




                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString()));
                            sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(5) + row + "-" + reportUtility.GetColumnNameForXls(6) + row + ")";
                            sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;

                            reportUtility.SetText(ref sheet, row, 8, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 9, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, 10].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(10) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(8) + row + "-" + reportUtility.GetColumnNameForXls(9) + row + ")";
                            sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 10].VerticalAlignment = ExcelVAlign.VAlignTop;
                        }
                        else
                        {
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(5) + row + "-" + reportUtility.GetColumnNameForXls(6) + row + ")";
                            sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
                        }
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                        row++;
                    }
                }


                reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
                sheet.Range[row, colDebit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colDebit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colDebit) + (row - 1) + ")";
                sheet.Range[row, colDebit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[row, colCredit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colCredit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colCredit) + (row - 1) + ")";
                sheet.Range[row, colCredit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                sheet.Range[row, 7].Formula = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, 7].CellStyle.Font.Bold = true;
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    sheet.Range[row, 10].Formula = "=" + reportUtility.GetColumnNameForXls(10) + (row - 1);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 10].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";

                sheet.Range[11, 4, row, 4].WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Cash Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();


                //sheet[row, 3].ColumnWidth = 50;
                //sheet[row, 4].ColumnWidth = 3;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;


                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetCashLedgerDataByplant(string companyGroupId, string companyId, string plantId, string cashMasterId, string fromDate, string toDate)
        {
            var cmdText = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                        DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                        DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        DECLARE @cashMasterId VARCHAR(10)='" + cashMasterId + @"';
                        SELECT V.VoucherNo, V.PostingDate, V.CurrencyId,
                         VD.DrAmount ,
                         VD.CrAmount 
						 , V.Narration
                        , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount
						 ,OtherSide = concat( STUFF((select distinct ','+XPP.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
                    where XVD.VoucherId=V.Id AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.AccountTitle from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join mst.BankMaster XEI ON XEI.id=XVD.BankMasterId
					LEFT JOIN HKP.Bank BX ON BX.Id=XEI.BankId
                    where XVD.VoucherId=V.Id AND XVD.BankMasterId <>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    
                    ,STUFF((select distinct ','+XEI.EmployeeName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
                    where XVD.VoucherId=V.Id AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XCM.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
                    where XVD.VoucherId=V.Id AND XVD.CashMasterId!=vd.CashMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XA.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.Activity XA ON XA.Id=XVD.ActivityId
                    where XVD.VoucherId=V.Id AND XVD.CashMasterId IS NULL AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                        FROM  [TRN].[VoucherDetail] AS VD 
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND VD.CashMasterId=@cashMasterId AND V.SourceType!='OpeningBalance'
						 AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"' AND V.SourceType!='OpeningBalance' AND VD.LoanSetOffGroupNo IS NULL
                        UNION ALL
                        SELECT V.VoucherNo, V.PostingDate, V.CurrencyId,
                         VD.DrAmount ,
                         VD.CrAmount 
						 , V.Narration
                        , CC.CompanyCurrencyDrAmount, CC.CompanyCurrencyCrAmount
						, OtherSide=CASE 
	                        WHEN P.UserName<>'' THEN P.UserName
							WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
	                        WHEN CM.UserName<>'' THEN CM.UserName
	                        ELSE ''	END
                        FROM  [TRN].[VoucherDetail] AS VD 
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND VD.CashMasterId=@cashMasterId AND V.SourceType!='OpeningBalance'
						 AND V.PostingDate > '" + fromDate + @"' AND V.SourceType='OpeningBalance' AND VD.LoanSetOffGroupNo IS NULL
                        UNION ALL
						 SELECT  VoucherNo=STUFF((select distinct ','+XV.VoucherNo from
							trn.VoucherDetail XVD 
							LEFT JOIN TRN.Voucher XV ON XVD.VoucherId=XV.Id
							where  XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						 , V.PostingDate, V.CurrencyId,
                         SUM(VD.DrAmount) DrAmount,
                         SUM(VD.CrAmount )CrAmount
						 , V.Narration
                        , SUM(CC.CompanyCurrencyDrAmount)CompanyCurrencyDrAmount, SUM(CC.CompanyCurrencyCrAmount)CompanyCurrencyCrAmount
						 ,OtherSide = concat( STUFF((select distinct ','+XPP.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.PartyPlant XPP ON XPP.Id=XVD.PartyPlantId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.PartyPlantId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XEI.AccountTitle from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join mst.BankMaster XEI ON XEI.id=XVD.BankMasterId
					LEFT JOIN HKP.Bank BX ON BX.Id=XEI.BankId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.BankMasterId <>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    
                    ,STUFF((select distinct ','+XEI.EmployeeName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join dbo.EmployeeInformation XEI ON XEI.SystemId=XVD.EmployeeId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.EmployeeId<>'' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XCM.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join MST.CashMaster XCM ON XCM.Id=XVD.CashMasterId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.CashMasterId!=vd.CashMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                    ,STUFF((select distinct ','+XA.UserName from
                    TRN.VoucherDetail AS XVD
                    left join TRN.Voucher XV ON XV.Id=XVD.VoucherId
                    left join HKP.Activity XA ON XA.Id=XVD.ActivityId
                    where XVD.LoanSetOffGroupNo=VD.LoanSetOffGroupNo AND XVD.CashMasterId IS NULL AND XVD.EmployeeId IS NULL AND XVD.PartyPlantId IS NULL for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''))
                        FROM  [TRN].[VoucherDetail] AS VD 
                        LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                        LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                        LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                        ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND VD.CashMasterId=@cashMasterId AND V.SourceType!='OpeningBalance'
                        AND V.PostingDate BETWEEN '" + fromDate + "' AND '" + toDate + @"' AND V.SourceType!='OpeningBalance' AND VD.LoanSetOffGroupNo <>''
						 GROUP BY  V.PostingDate, V.CurrencyId, V.Narration,VD.LoanSetOffGroupNo,VD.CashMasterId
						 ORDER BY V.PostingDate ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }
        #region Cash Receipt Payment Report
        public IWorkbook GetCashReceiptPaymentReport(string companyGroupId, string companyId, string plantId, string plantName, string cashMasterId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var colLast = 0;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "CashReceiptPaymentReport";

                // Get BankMaster data
                var cashMaster = _cashJournalService.GetCashMaster(cashMasterId);

                // Set Header
                int colCash = 0; colCash++;
                int colCaptionCash = colCash;
                reportUtility.SetMasterHeaderText(ref sheet, row, colCash, "Cash");
                colCash++;
                int colCashMarge = colCash;
                sheet.Range[reportUtility.GetColumnNameForXls(colCaptionCash) + row + ":" + reportUtility.GetColumnNameForXls(colCashMarge) + row].Merge();
                colCash++; //3
                int colCashName = colCash;
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colCashName, cashMaster["CashName"].ToString());
                //sheet[row, colCashName].ColumnWidth = 150;

                colCash++; //4
                int colGL = colCash;
                reportUtility.SetMasterHeaderText(ref sheet, row, colGL, "GL"); //4
                //reportUtility.SetHeaderText(ref sheet, row, 6, "Amount", 15, ExcelHAlign.HAlignRight);

                colCash++; //5
                           // colGL++; //5
                int colGlValue = colCash;
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colGlValue, cashMaster["GLGeneralInfoCode"] + " - " + cashMaster["GLGeneralInfoName"]);
                // colGlValue++;  //6col
                colCash++; //6
                int colGLMarge = colCash;
                sheet.Range[reportUtility.GetColumnNameForXls(colGlValue) + row + ": " + reportUtility.GetColumnNameForXls(colGLMarge) + row].Merge();


                row++;
                int colCashCurrency = colCaptionCash;
                //colCash++;
                // int colCCMarge = colCashCurrency + 1;
                int colCCMarge = colCashMarge;


                reportUtility.SetMasterHeaderText(ref sheet, row, colCashCurrency, "Cash Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(colCashCurrency) + row + ":" + reportUtility.GetColumnNameForXls(colCCMarge) + row].Merge();
                int colCurrencyCode = colCashName;
                reportUtility.SetMiddleAlignmentText(ref sheet, row, colCurrencyCode, cashMaster["CurrencyCode"].ToString());




                // Detail Header
                row++;
                int colVoucherNo = colCaptionCash;
                reportUtility.SetHeaderText(ref sheet, row, colVoucherNo, "Voucher No", 15);

                // colCash++;
                int colPostingDate = colCashMarge;
                reportUtility.SetHeaderText(ref sheet, row, colPostingDate, "Posting Date", 12);

                //colCash++;
                int colOtherSide = colCashName;
                reportUtility.SetHeaderText(ref sheet, row, colOtherSide, "Account Name", 40); //3
                sheet[row, colOtherSide].ColumnWidth = 35;

                // colCash++;
                int colNaration = colGL;
                reportUtility.SetHeaderText(ref sheet, row, colNaration, "Narration", 40);
                sheet[row, colNaration].ColumnWidth = 40;
                //sheet.Range[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

                // colCash++;
                int colHAmount = colGlValue; //col5
                reportUtility.SetHeaderText(ref sheet, row, colHAmount, "HAmount", 12, ExcelHAlign.HAlignRight); //5
                sheet[row, colHAmount].ColumnWidth = 15;

                //colCash++;
                int colAmount = colGLMarge;
                reportUtility.SetHeaderText(ref sheet, row, colAmount, "Amount", 15, ExcelHAlign.HAlignRight);

                colGLMarge++; //7
                int colBalance = colGLMarge;
                reportUtility.SetHeaderText(ref sheet, row, colBalance, "Balance", 15, ExcelHAlign.HAlignRight);

                colLast = colBalance;
                sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Thin);
                sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Thin);


                row++;
                int colOB = colVoucherNo;
                reportUtility.SetText(ref sheet, row, colOB, "Opening Balance", true);
                int colOBMarge = colNaration;
                sheet.Range[reportUtility.GetColumnNameForXls(colOB) + row + ":" + reportUtility.GetColumnNameForXls(colOBMarge) + row].Merge();

                // Get Cash opening balance data.
                var obVal = _cashJournalService.GetCashOpeningBalanceLedgerData(companyGroupId, companyId, plantId, cashMasterId, fromDate);
                int colOBValue = colBalance;
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    var ob = Convert.ToDouble(obVal[0]["OB"]);
                    reportUtility.SetText(ref sheet, row, colOBValue, ob, true);

                }
                //sheet.Range[row, 1, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;

                row++;
                reportUtility.SetHeaderText(ref sheet, row, colVoucherNo, "Add Receipt: ", 10, ExcelHAlign.HAlignRight);
                //reportUtility.SetHeaderText(ref sheet, row, 1, "Balance", 15, ExcelHAlign.HAlignRight);
                // sheet.Range[row, 1].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
                // sheet.Range[row, 1, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Hair;
                //  sheet.Range[row, 1, row, 1].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                //sheet.Range[row, 1, row, 4].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 1, row, 4].BorderInside(ExcelLineStyle.Thin);

                row++;


                var ledgerReceiptData = GetReceiptData(companyGroupId, companyId, plantId, cashMasterId, fromDate, toDate);
                int formulaReceiptStartRow = row;
                if (ledgerReceiptData.Rows.Count > 0)
                {
                    for (int i = 0; i < ledgerReceiptData.Rows.Count; i++)
                    {
                        reportUtility.SetText(ref sheet, row, colVoucherNo, ledgerReceiptData.Rows[i]["VoucherNo"].ToString());
                        reportUtility.SetText(ref sheet, row, colPostingDate, Convert.ToDateTime(ledgerReceiptData.Rows[i]["PostingDate"].ToString()).ToString("dd-MMM-yyyy"));
                        reportUtility.SetText(ref sheet, row, colOtherSide, ledgerReceiptData.Rows[i]["OtherSide"].ToString());
                        reportUtility.SetText(ref sheet, row, colNaration, ledgerReceiptData.Rows[i]["Narration"].ToString());

                        // reportUtility.SetText(ref sheet, row, 5, ledgerReceiptData.Rows[i]["Narration"].ToString());

                        sheet[row, colAmount].Number = clsStaticInfo.dbl(ledgerReceiptData.Rows[i]["ReceiptAmount"].ToString());
                        sheet[row, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                        //sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, colAmount].VerticalAlignment = ExcelVAlign.VAlignTop;
                        sheet[row, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                        sheet[row, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                        //reportUtility.SetText(ref sheet, row, 4, ledgerReceiptData.Rows[i]["Narration"].ToString(), false, true);
                        sheet.Range[row, colNaration].WrapText = true;

                        sheet.Range[formulaReceiptStartRow, colNaration, row, colNaration].WrapText = true;


                        sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                        row++;

                    }

                }

                int formulaReceiptEndRow = row;
                sheet.Range[formulaReceiptEndRow - 1, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colAmount) + (formulaReceiptStartRow) + ":" + reportUtility.GetColumnNameForXls(colAmount) + (formulaReceiptEndRow - 1) + ")"   /* + "+" + reportUtility.GetColumnNameForXls(colOB) + 9*/ ;
                sheet.Range[formulaReceiptEndRow - 1, colBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[formulaReceiptEndRow - 1, colBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[formulaReceiptEndRow - 1, colBalance].CellStyle.Font.Bold = true;

                row++;
                reportUtility.SetHeaderText(ref sheet, row, colVoucherNo, "Less Payment: ", 15, ExcelHAlign.HAlignRight);
                sheet[row, colVoucherNo].BorderAround(ExcelLineStyle.None);
                //sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();
                //sheet.Range[row, 1, row, 4].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 1, row, 4].BorderInside(ExcelLineStyle.Thin);

                row++;



                var ledgerPaymentData = GetLedgerPaymentData(companyGroupId, companyId, plantId, cashMasterId, fromDate, toDate);
                int formulaPaymentStartRow = row;
                if (ledgerPaymentData.Rows.Count > 0)
                {
                    for (int i = 0; i < ledgerPaymentData.Rows.Count; i++)
                    {
                        reportUtility.SetText(ref sheet, row, colVoucherNo, ledgerPaymentData.Rows[i]["VoucherNo"].ToString());
                        reportUtility.SetText(ref sheet, row, colPostingDate, Convert.ToDateTime(ledgerPaymentData.Rows[i]["PostingDate"].ToString()).ToString("dd-MMM-yyyy"));
                        reportUtility.SetText(ref sheet, row, colOtherSide, ledgerPaymentData.Rows[i]["OtherSide"].ToString());
                        reportUtility.SetText(ref sheet, row, colNaration, ledgerPaymentData.Rows[i]["Narration"].ToString());
                        // reportUtility.SetText(ref sheet, row, 3, ledgerPaymentData.Rows[i]["Narration"].ToString(), false, true);
                        sheet.Range[row, colNaration].WrapText = true;
                        //sheet.Range[row, 3,row,4].WrapText = true;
                        // reportUtility.SetText(ref sheet, row, 5, ledgerPaymentData.Rows[i]["PaymentAmount"].ToString());
                        sheet[row, colAmount].Number = clsStaticInfo.dbl(ledgerPaymentData.Rows[i]["PaymentAmount"].ToString());
                        sheet[row, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                        //sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, colAmount].VerticalAlignment = ExcelVAlign.VAlignTop;
                        sheet[row, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";




                        sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                        row++;

                    }

                }
                int formulaPaymentEndRow = row;
                sheet.Range[formulaPaymentEndRow - 1, colBalance].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colAmount) + (formulaPaymentStartRow) + ":" + reportUtility.GetColumnNameForXls(colAmount) + (formulaPaymentEndRow - 1) + ")"   /* + "+" + reportUtility.GetColumnNameForXls(colOB) + 9*/ ;
                sheet.Range[formulaPaymentEndRow - 1, colBalance].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[formulaPaymentEndRow - 1, colBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[formulaPaymentEndRow - 1, colBalance].CellStyle.Font.Bold = true;

                reportUtility.SetText(ref sheet, row, colVoucherNo, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colVoucherNo) + row + ":" + reportUtility.GetColumnNameForXls(colHAmount) + row].Merge(); //[formulaReceiptEndRow - 1, 7

                sheet[row, colBalance].Formula = reportUtility.GetColumnNameForXls(colOBValue) + (9) + "+" + reportUtility.GetColumnNameForXls(colBalance) + (formulaReceiptEndRow - 1) + "-" + reportUtility.GetColumnNameForXls(colBalance) + (formulaPaymentEndRow - 1);
                sheet[row, colBalance].NumberFormat = clsStaticInfo.NumberFormat();
                //sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, colBalance].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet[row, colBalance].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet.Range[row, colBalance].CellStyle.Font.Bold = true;



                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Cash Receipt & Payment", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + "5:" + reportUtility.GetColumnNameForXls(colLast) + 5.ToString()].Merge();



                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);

                sheet.ShowColumn(5, false);
                //sheet.HideColumn(5);
                //sheet[1, 5].ColumnWidth = 0; 
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable GetReceiptData(string companyGroupId, string companyId, string plantId, string cashMasterId, string fromDate, string toDate)
        {
            try
            {
                var sql = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                    DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                    DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                    DECLARE @cashMasterId VARCHAR(10)='" + cashMasterId + @"';


                    SELECT V.VoucherNo, V.PostingDate AS PostingDate, V.CurrencyId
                    ,SUM(ISNULL(VD.CrAmount,0)) ReceiptAmount
                    ,SUM(ISNULL(VD.DrAmount,0)) PaymentAmount
                    ,B.UserName BudgetName
                    ,VD.Narration
                    , SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyDrAmount,0)) CompanyCurrencyCrAmount
                    , OtherSide=CASE
                    WHEN P.UserName<>'' THEN P.UserName
                    WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                    WHEN CM.UserName<>'' THEN CM.UserName
                    WHEN EI.EmployeeName <>'' THEN A.UserName+' - '+EI.EmployeeName
                    ELSE A.UserName END

                    FROM [TRN].[VoucherDetail] AS VD
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                    LEFT JOIN [MST].[BudgetMaster] AS BDM ON BDM.Id=VD.BudgetMasterId
                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BDM.BudgetId
                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                    LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=VD.EmployeeId
                    JOIN (SELECT VoucherId FROM TRN.VoucherDetail VVD WHERE VVD.CashMasterId=@cashMasterId ) VDD ON VDD.VoucherId=VD.VoucherId
                    LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, ISNULL(VDC.DrAmount,0) AS CompanyCurrencyDrAmount, ISNULL(VDC.CrAmount,0) AS CompanyCurrencyCrAmount
                    FROM [TRN].[VoucherDetailCurrency] AS VDC
                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND (isnull(VD.CashMasterId,'')='' OR (isnull(VD.CashMasterId,'')<>'' AND VD.CashMasterId<>@cashMasterId))
                    AND V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND V.SourceType != 'OpeningBalance' and VD.CrAmount>0
                    GROUP BY V.VoucherNo, V.PostingDate, V.CurrencyId ,B.UserName
                    ,VD.Narration,P.UserName, BM.AccountTitle,CM.UserName,EI.EmployeeName,A.UserName

                    UNION ALL
                    SELECT V.VoucherNo,V.PostingDate AS PostingDate, V.CurrencyId
                    ,SUM(ISNULL(VD.CrAmount,0)) ReceiptAmount
                    ,SUM(ISNULL(VD.DrAmount,0)) PaymentAmount
                    ,B.UserName BudgetName
                    ,VD.Narration
                    , SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyDrAmount,0)) CompanyCurrencyCrAmount
                    , OtherSide=CASE
                    WHEN P.UserName<>'' THEN P.UserName
                    WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                    WHEN CM.UserName<>'' THEN CM.UserName
                    WHEN EI.EmployeeName <>'' THEN EI.EmployeeName
                    ELSE A.UserName END

                    FROM [TRN].[VoucherDetail] AS VD
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                    LEFT JOIN [MST].[BudgetMaster] AS BDM ON BDM.Id=VD.BudgetMasterId
                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BDM.BudgetId
                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                    LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=VD.EmployeeId
                    JOIN (SELECT VoucherId FROM TRN.VoucherDetail VVD WHERE VVD.CashMasterId=@cashMasterId ) VDD ON VDD.VoucherId=VD.VoucherId
                    LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, ISNULL(VDC.DrAmount,0) AS CompanyCurrencyDrAmount, ISNULL(VDC.CrAmount,0) AS CompanyCurrencyCrAmount
                    FROM [TRN].[VoucherDetailCurrency] AS VDC
                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND (isnull(VD.CashMasterId,'')='' OR (isnull(VD.CashMasterId,'')<>'' AND VD.CashMasterId<>@cashMasterId))
                    AND V.PostingDate > '" + fromDate + @"' AND V.SourceType = 'OpeningBalance' and VD.CrAmount>0
                    GROUP BY V.VoucherNo, V.PostingDate, V.CurrencyId ,B.UserName
                    ,VD.Narration,P.UserName, BM.AccountTitle,CM.UserName,EI.EmployeeName,A.UserName
                    ORDER BY PostingDate ASC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }


        private DataTable GetLedgerPaymentData(string companyGroupId, string companyId, string plantId, string cashMasterId, string fromDate, string toDate)
        {
            try
            {
                var sql = @"DECLARE @companyGroupId VARCHAR(10)='" + companyGroupId + @"';
                    DECLARE @companyId VARCHAR(10)='" + companyId + @"';
                    DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                    DECLARE @cashMasterId VARCHAR(10)='" + cashMasterId + @"';


                    SELECT V.VoucherNo, V.PostingDate AS PostingDate, V.CurrencyId
                    ,SUM(ISNULL(VD.CrAmount,0)) ReceiptAmount
                    ,SUM(ISNULL(VD.DrAmount,0)) PaymentAmount
                    ,B.UserName BudgetName                
                    ,VD.Narration
                    , SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyDrAmount,0)) CompanyCurrencyCrAmount
                    , OtherSide=CASE
                    WHEN P.UserName<>'' THEN P.UserName
                    WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                    WHEN CM.UserName<>'' THEN CM.UserName
                    WHEN EI.EmployeeName <>'' THEN A.UserName+' - '+EI.EmployeeName
                    ELSE A.UserName END

                    FROM [TRN].[VoucherDetail] AS VD
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                    LEFT JOIN [MST].[BudgetMaster] AS BDM ON BDM.Id=VD.BudgetMasterId
                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BDM.BudgetId
                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                    LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=VD.EmployeeId
                    JOIN (SELECT VoucherId FROM TRN.VoucherDetail VVD WHERE VVD.CashMasterId=@cashMasterId ) VDD ON VDD.VoucherId=VD.VoucherId
                    LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, ISNULL(VDC.DrAmount,0) AS CompanyCurrencyDrAmount, ISNULL(VDC.CrAmount,0) AS CompanyCurrencyCrAmount
                    FROM [TRN].[VoucherDetailCurrency] AS VDC
                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND (isnull(VD.CashMasterId,'')='' OR (isnull(VD.CashMasterId,'')<>'' AND VD.CashMasterId<>@cashMasterId))
                    AND V.PostingDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND V.SourceType != 'OpeningBalance' and VD.DrAmount>0
                    GROUP BY V.VoucherNo, V.PostingDate, V.CurrencyId ,B.UserName
                    ,VD.Narration,P.UserName, BM.AccountTitle,CM.UserName,EI.EmployeeName,A.UserName

                    UNION ALL
                    SELECT V.VoucherNo, V.PostingDate AS PostingDate, V.CurrencyId
                    ,SUM(ISNULL(VD.CrAmount,0)) ReceiptAmount
                    ,SUM(ISNULL(VD.DrAmount,0)) PaymentAmount
                    ,B.UserName BudgetName
                    ,VD.Narration
                    , SUM(ISNULL(CC.CompanyCurrencyCrAmount,0)) CompanyCurrencyDrAmount, SUM(ISNULL(CC.CompanyCurrencyDrAmount,0)) CompanyCurrencyCrAmount
                    , OtherSide=CASE
                    WHEN P.UserName<>'' THEN P.UserName
                    WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                    WHEN CM.UserName<>'' THEN CM.UserName
                    WHEN EI.EmployeeName <>'' THEN EI.EmployeeName
                    ELSE A.UserName END

                    FROM [TRN].[VoucherDetail] AS VD
                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                    LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                    LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                    LEFT JOIN [HKP].[Party] AS P ON P.Id=VD.PartyId
                    LEFT JOIN [MST].[BudgetMaster] AS BDM ON BDM.Id=VD.BudgetMasterId
                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BDM.BudgetId
                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=VD.ActivityId
                    LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=VD.EmployeeId
                    JOIN (SELECT VoucherId FROM TRN.VoucherDetail VVD WHERE VVD.CashMasterId=@cashMasterId ) VDD ON VDD.VoucherId=VD.VoucherId
                    LEFT JOIN (SELECT VDC.VoucherId, VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, ISNULL(VDC.DrAmount,0) AS CompanyCurrencyDrAmount, ISNULL(VDC.CrAmount,0) AS CompanyCurrencyCrAmount
                    FROM [TRN].[VoucherDetailCurrency] AS VDC
                    JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
                    WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId=@companyId
                    ) AS CC ON CC.VoucherId=VD.VoucherId AND CC.VoucherDetailId=VD.Id
                    WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId=@companyGroupId AND V.CompanyId=@companyId AND V.PlantId=@plantId AND (isnull(VD.CashMasterId,'')='' OR (isnull(VD.CashMasterId,'')<>'' AND VD.CashMasterId<>@cashMasterId))
                    AND V.PostingDate > '" + fromDate + @"' AND V.SourceType = 'OpeningBalance' and VD.DrAmount>0
                    GROUP BY V.VoucherNo,V.PostingDate, V.CurrencyId ,B.UserName
                    ,VD.Narration,P.UserName, BM.AccountTitle,CM.UserName,EI.EmployeeName,A.UserName
                    ORDER BY PostingDate ASC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion Cash Receipt Payment Report

        public IWorkbook GetAdvanceCashBookReport(string companyGroupId, string companyId, string plantId, string plantName, string cashMasterId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                // Get BankMaster data
                var cashMaster = _cashJournalService.GetCashMaster(cashMasterId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, cashMaster["CashName"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, cashMaster["GLGeneralInfoCode"] + " - " + cashMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Cash Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, cashMaster["CurrencyCode"].ToString());

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 4, "Cash Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(7) + row].Merge();

                colLast = 7;
                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                var cashCurrencyId = cashMaster["CurrencyId"].ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(6) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();
                    colLast = 9;
                }

                // Detail Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, 1, "Voucher No", 12);
                reportUtility.SetHeaderText(ref sheet, row, 2, "Account Name", 32);
                reportUtility.SetHeaderText(ref sheet, row, 3, "Narration", 70);
                reportUtility.SetHeaderText(ref sheet, row, 4, "Debit", 9, ExcelHAlign.HAlignRight); int colDebit = 4;
                reportUtility.SetHeaderText(ref sheet, row, 5, "Credit", 9, ExcelHAlign.HAlignRight); int colCredit = 5;
                reportUtility.SetHeaderText(ref sheet, row, 6, "Balance", 12, ExcelHAlign.HAlignRight);
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 7, "Debit", 60, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 8, "Credit", 10, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 9, "Balance", ExcelHAlign.HAlignRight);
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);
                row++;
               
                // Get Cash transaction data.
                var ledgerData = _cashJournalService.GetAdvanceCashBookData(companyGroupId, companyId, plantId, cashMasterId, fromDate, toDate);
                var obVal = _cashJournalService.GetCashOpeningBalanceLedgerData(companyGroupId, companyId, plantId, cashMasterId, fromDate);
                if (ledgerData.Rows.Count > 0)
                {
                    var dt = ledgerData.AsEnumerable().OrderBy(r => Convert.ToDateTime(r["PostingDate"]))
                            .GroupBy(r => new { PostingDate = r["PostingDate"] })
                            .Select(g => g.OrderBy(r => r["PostingDate"]).First())
                            .CopyToDataTable();
                    var isOB = true;
                    var lastClosing = string.Empty;
                    int StartRow = 0;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        var data = ledgerData.AsEnumerable()
                            .Where(r => r.Field<string>("PostingDate") == dt.Rows[j]["PostingDate"].ToString())
                            .OrderBy(r => r["VoucherNo"])
                            .CopyToDataTable();

                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                        reportUtility.SetText(ref sheet, row, 1, "As On " + dt.Rows[j]["PostingDate"]);
                        sheet.Range[row, 1].CellStyle.Font.Bold = true;
                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].BorderAround(ExcelLineStyle.Hair);
                        row++;
                        StartRow = row;
                        reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

                        // Get Cash opening balance data.
                        if (obVal.Count > 0 && isOB)
                        {
                            // Set Opening Balance
                            var ob = Convert.ToDouble(obVal[0]["OB"]);
                            reportUtility.SetText(ref sheet, row, 6, ob, true);

                            sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                                reportUtility.SetText(ref sheet, row, 8, ob, true);
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            isOB = false;
                        }
                        else
                        {
                            reportUtility.SetFormula(ref sheet, row, 6, lastClosing, true);
                            sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();

                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        }

                        row++;
                        for (int i = 0; i < data.Rows.Count; i++)
                        {
                            reportUtility.SetText(ref sheet, row, 1, data.Rows[i]["VoucherNo"].ToString());
                            reportUtility.SetText(ref sheet, row, 2, data.Rows[i]["OtherSide"].ToString());
                            reportUtility.SetTextWrapText(ref sheet, row, 3, data.Rows[i]["Narration"].ToString());
                            reportUtility.SetText(ref sheet, row, 4, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, 6].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(4) + row + "-" + reportUtility.GetColumnNameForXls(5) + row + ")";
                            sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;

                            // Base currency checking
                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, 8, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                                sheet.Range[row, 9].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(9) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(7) + row + "-" + reportUtility.GetColumnNameForXls(8) + row + ")";
                                sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                                sheet.Range[row, 9].VerticalAlignment = ExcelVAlign.VAlignTop;
                            }
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                            row++;
                        }
                        reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();
                        sheet.Range[row, colDebit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colDebit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colDebit) + (row - 1) + ")";
                        sheet.Range[row, colDebit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                        sheet.Range[row, colCredit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colCredit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colCredit) + (row - 1) + ")";
                        sheet.Range[row, colCredit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                        sheet.Range[row, 6].Formula = "=" + reportUtility.GetColumnNameForXls(6) + (row - 1);
                        sheet.Range[row, 6].ColumnWidth = 40;
                        lastClosing = "=" + reportUtility.GetColumnNameForXls(6) + (row - 1);
                        sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                        sheet.Range[row, 6].CellStyle.Font.Bold = true;
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            sheet.Range[row, 9].Formula = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                            sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatNegativeSignDelimeterDecimalTwo();
                            sheet.Range[row, 9].CellStyle.Font.Bold = true;
                        }
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"  Dr\", \"  Cr\")";
                        row++;
                    }
                }

                sheet.UsedRange.AutofitRows();
                sheet.Range[row, 4].ColumnWidth = 20;
                sheet.Range[row, 5].ColumnWidth = 20;
                sheet.Range[row, 6].ColumnWidth = 20;


                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Cash Book", companyId,plantId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}