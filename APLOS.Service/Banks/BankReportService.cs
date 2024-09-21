using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Currencies;
using Library.Service.Extension;
using Library.Service.Extension.Accounts;
using Library.Service.Helpers;
using Library.Service.Organizations;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Drawing.Printing;

namespace Library.Service.Banks
{
    public class BankReportService : IBankReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IBankJournalNewService _bankJournalService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;

        public BankReportService(
            ISqlRepository sqlRepository
            , IBankJournalNewService bankJournalService
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService
            )
        {
            _sqlRepository = sqlRepository;
            _bankJournalService = bankJournalService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
        }

        public IWorkbook xGetPaymentByBankReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = _bankJournalService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, sourceType);
            // Set report Name
            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = _bankJournalService.GetBankJournalDetail(companyGroupId, companyId, plantId, voucherId, sourceType);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 2, header["DocRefNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank");
            reportUtility.SetText(ref sheet, row, 2, header["BankName"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Branch");
            reportUtility.SetText(ref sheet, row, 4, header["BankBranchName"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account No");
            reportUtility.SetText(ref sheet, row, 2, header["AccountNumber"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Account Title");
            reportUtility.SetText(ref sheet, row, 4, header["AccountTitle"].ToString(), false, true);

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
            reportUtility.SetHeaderText(ref sheet, row, 3, "Particulars", 12);
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
                reportUtility.SetHeaderText(ref sheet, row, 4, "Debit", 14, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Credit", 14, ExcelHAlign.HAlignRight);
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
                        glName = dsLocal.Rows[i]["GLGeneralInfoName"].ToString();

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
                    sheet.Range[row, 4].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, 4].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 4].CellStyle.Font.Bold = true;
                    sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 4].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 5].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 5].CellStyle.Font.Bold = true;
                    sheet.Range[row, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 5].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 6].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 6].CellStyle.Font.Bold = true;
                    sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 6].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 6].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + 12 + ":" + reportUtility.GetColumnNameForXls(7) + (row - 1) + ")";
                    sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 7].CellStyle.Font.Bold = true;
                    sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 7].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 7].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, 4].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, 4].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 4].CellStyle.Font.Bold = true;
                    sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 4].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 5].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 5].CellStyle.Font.Bold = true;
                    sheet.Range[row, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 5].BorderAround(ExcelLineStyle.Hair);
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



        public IWorkbook GetPaymentByBankReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Bank Journal";

            // var header = GetAdvanceJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.AdvanceJournalVoucher);
            //var header = _bankJournalService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);
            BankExtensionService bankExtensionService = new BankExtensionService();

            var header = bankExtensionService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            //  var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);
            var dsLocal = bankExtensionService.GetBankJournalDetail(companyGroupId, companyId, plantId, voucherId, SourceType.BankJournal);

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


            int colCheckNo = colVoucherNo;
            int colCheckNoValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckNo, "CheckNo");
            reportUtility.SetText(ref sheet, row, colCheckNoValue, header["CheckNumber"].ToString());


            int colCheckDate = colVoucherDate;
            int colCheckDateValue = colVoucherDateValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colCheckDate, "Check Date");
            reportUtility.SetText(ref sheet, row, colCheckDateValue, header["CheckDate"].ToString());
            row++;

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
            row++;

            int colEntityNo = colVoucherNo;
            int colEntityNoValue = colVoucherNoValue;
            reportUtility.SetMasterHeaderText(ref sheet, row, colEntityNo, "Entity");
            reportUtility.SetText(ref sheet, row, colEntityNoValue, header["Entity"].ToString());


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


                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Bank Journal", companyId, plantId, plantName, null);


                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);

            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Bank Journal", companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }


        public IWorkbook GetBankJournalReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = _bankJournalService.GetBankJournalHeader(companyGroupId, companyId, plantId, voucherId, sourceType);
            // Set report Name
            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = _bankJournalService.GetBankJournalDetail(companyGroupId, companyId, plantId, voucherId, sourceType);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 4, header["VoucherDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 2, header["DocRefNo"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "DocDate");
            reportUtility.SetText(ref sheet, row, 4, header["DocDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Status");
            reportUtility.SetText(ref sheet, row, 4, header["Status"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank");
            reportUtility.SetText(ref sheet, row, 2, header["BankName"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Branch");
            reportUtility.SetText(ref sheet, row, 4, header["BankBranchName"].ToString());
            sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account No");
            reportUtility.SetText(ref sheet, row, 2, header["AccountNumber"].ToString());
            reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Account Title");
            reportUtility.SetText(ref sheet, row, 4, header["AccountTitle"].ToString());
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
            reportUtility.SetHeaderText(ref sheet, row, 3, "", 12);
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
                reportUtility.SetHeaderText(ref sheet, row, 4, "Debit", 14, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Credit", 14, ExcelHAlign.HAlignRight);
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
                        glName = dsLocal.Rows[i]["GLGeneralInfoName"].ToString();

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
                    sheet.Range[row, 4].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, 4].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 4].CellStyle.Font.Bold = true;
                    sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 4].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 5].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 5].CellStyle.Font.Bold = true;
                    sheet.Range[row, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 5].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 6].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + 12 + ":" + reportUtility.GetColumnNameForXls(6) + (row - 1) + ")";
                    sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 6].CellStyle.Font.Bold = true;
                    sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 6].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 6].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + 12 + ":" + reportUtility.GetColumnNameForXls(7) + (row - 1) + ")";
                    sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 7].CellStyle.Font.Bold = true;
                    sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 7].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 7].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, 4].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(4) + 12 + ":" + reportUtility.GetColumnNameForXls(4) + (row - 1) + ")";
                    sheet.Range[row, 4].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 4].CellStyle.Font.Bold = true;
                    sheet.Range[row, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 4].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, 5].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(5) + 12 + ":" + reportUtility.GetColumnNameForXls(5) + (row - 1) + ")";
                    sheet.Range[row, 5].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 5].CellStyle.Font.Bold = true;
                    sheet.Range[row, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, 5].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, 5].BorderAround(ExcelLineStyle.Hair);
                }

                sheet.Range[13, 1, row - 1, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[13, 1, row - 1, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency)//&& _plantService.Find(plantId).IsShowFCInWord
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

        public IWorkbook GetBankOpeningBalanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId, bool isCompanyCurrency)
        {
            try
            {
                var row = 5;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Report";

                // Get bank transaction data.
                var fiscalYear = _sqlRepository.GetData("SELECT FiscalYearCode, FiscalYearName, StartDate, EndDate FROM [SCS].[FiscalYear] WHERE Id='" + fiscalYearId + "'");
                var ledgerData = _bankJournalService.GetBankLedgerData(companyGroupId, companyId, plantId, null, null, null, true, fiscalYearId);

                if (ledgerData.Rows.Count > 0)
                {
                    // Set Header Column
                    row++;
                    reportUtility.SetHeaderText(ref sheet, row, 7, "Bank Currency", ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(7) + row + ":" + reportUtility.GetColumnNameForXls(8) + row].Merge();

                    colLast = 8;
                    _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                    if (isCompanyCurrency && !string.IsNullOrEmpty(companyCurrencyId))
                    {
                        reportUtility.SetHeaderText(ref sheet, row, 9, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                        sheet.Range[reportUtility.GetColumnNameForXls(9) + row + ":" + reportUtility.GetColumnNameForXls(10) + row].Merge();
                        colLast = 10;
                    }

                    // Detail Row Header
                    row++;
                    reportUtility.SetHeaderText(ref sheet, row, 1, "Bank Title", 32);
                    reportUtility.SetHeaderText(ref sheet, row, 2, "Bank Account", 15);
                    reportUtility.SetHeaderText(ref sheet, row, 3, "Voucher No", 12);
                    reportUtility.SetHeaderText(ref sheet, row, 4, "Posting Date", 12);
                    reportUtility.SetHeaderText(ref sheet, row, 5, "Narration", 26);
                    reportUtility.SetHeaderText(ref sheet, row, 6, "Bank Currency", 8);
                    reportUtility.SetHeaderText(ref sheet, row, 7, "Debit", 10);
                    reportUtility.SetHeaderText(ref sheet, row, 8, "Credit", 10);

                    if (isCompanyCurrency && !string.IsNullOrEmpty(companyCurrencyId))
                    {
                        reportUtility.SetHeaderText(ref sheet, row, 9, "Debit", 10, ExcelHAlign.HAlignRight);
                        reportUtility.SetHeaderText(ref sheet, row, 10, "Credit", 10, ExcelHAlign.HAlignRight);
                    }

                    row++;
                    if (ledgerData.Rows.Count > 0)
                    {
                        for (int i = 0; i < ledgerData.Rows.Count; i++)
                        {
                            reportUtility.SetText(ref sheet, row, 1, ledgerData.Rows[i]["AccountTitle"].ToString());
                            reportUtility.SetText(ref sheet, row, 2, ledgerData.Rows[i]["AccountNumber"].ToString());
                            reportUtility.SetText(ref sheet, row, 3, ledgerData.Rows[i]["VoucherNo"].ToString());
                            reportUtility.SetText(ref sheet, row, 4, ledgerData.Rows[i]["PostingDate"].ToString());
                            reportUtility.SetText(ref sheet, row, 5, ledgerData.Rows[i]["Narration"].ToString());
                            reportUtility.SetText(ref sheet, row, 6, ledgerData.Rows[i]["CurrencyCode"].ToString());
                            reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 8, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString()));

                            // Base currency checking
                            if (isCompanyCurrency && !string.IsNullOrEmpty(companyCurrencyId))
                            {
                                reportUtility.SetText(ref sheet, row, 9, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, 10, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            }
                            row++;
                        }
                    }
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                }
                else
                {
                    reportUtility.SetText(ref sheet, 6, colLast, "Data not found!", ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(1) + 6 + ":" + reportUtility.GetColumnNameForXls(colLast) + 6].Merge();
                }
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Bank Opening Balance Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, colLast, "Fiscal Year: " + fiscalYear["FiscalYearName"], ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(colLast) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetBankLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate, bool extended)
        {
            try
            {
                var row = 6;
                var colLast = 0;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                int xlsCol = 1;
                int colVoucherNo = 0;
                int colPostingDate = 0;
                //int colAccountName = 0;
                int colNarration = 0;
                int colParticulars = 0;

                int colDebit = 0;
                int colCredit = 0;
                int colBlance = 0;
                int colDrCr = 0;
                int colVoucherDetailId = 0;
                int colReconcileDate = 0;
                int colReconciliationStatus = 0;
                //int colLast = xlsCol;

                // Get BankMaster data
                var bankMaster = _bankJournalService.GetBankMaster(bankMasterId);

                // Set Header
                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank");
                // sheet.Range[row, 1, row, 2].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankMaster["BankName"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Branch");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["BankBranchName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account No");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankMaster["AccountNumber"].ToString());


                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Account Title");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["AccountTitle"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                var bankCurrencyCode = bankMaster["CurrencyCode"].ToString();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankCurrencyCode);

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["GLGeneralInfoCode"] + " - " + bankMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 5, "Bank Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ":" + reportUtility.GetColumnNameForXls(7) + row].Merge();
                sheet.Range[row, 5, row, 7].BorderAround(ExcelLineStyle.Thin);

                colLast = 8;

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyCode)
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(colLast) + row + ":" + reportUtility.GetColumnNameForXls(10) + row].Merge();
                    sheet.Range[row, 8, row, 10].BorderAround(ExcelLineStyle.Thin);
                    colLast = 11;
                }

                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Voucher No", 13); colVoucherNo = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Posting Date", 11); colPostingDate = xlsCol; xlsCol++;
                //reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Account Name", 32); colAccountName = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 25); colParticulars = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Narration", 25); colNarration = xlsCol; xlsCol++;


                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colCredit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Balance", 15, ExcelHAlign.HAlignRight); colBlance = xlsCol; xlsCol++;


                int colCompanyDr = 0;
                int colCompanyCr = 0;
                int colCompanyBlance = 0;
                // int colDrCr = 0;

                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyCode)
                {
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colCompanyDr = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colCompanyCr = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Balance", 15, ExcelHAlign.HAlignRight); colCompanyBlance = xlsCol; xlsCol++;
                }
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Dr/Cr", 5, ExcelHAlign.HAlignRight); colDrCr = xlsCol;
                if (extended == true)
                {
                    xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Id", 15, ExcelHAlign.HAlignCenter); colVoucherDetailId = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Reconciliation Date", 20); colReconcileDate = xlsCol; xlsCol++;
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Reconciliation Status", 20); colReconciliationStatus = xlsCol;

                }
                row++;
                reportUtility.SetText(ref sheet, row, 2, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(colPostingDate) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

                // Get bank opening balance data.
                BankExtensionService bankExtensionService = new BankExtensionService();
                //var obVal = _bankJournalService.GetBankOpeningBalanceLedgerData(companyGroupId, companyId, plantId, bankMasterId, fromDate);
                var obVal = bankExtensionService.GetBankOpeningBalanceLedgerData(companyGroupId, companyId, plantId, bankMasterId, fromDate);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(obVal[0]["OB"]), true);
                    sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyCode)
                        reportUtility.SetText(ref sheet, row, 10, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    colLast = 8;
                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyCode)
                    {
                        colLast = 11;
                    }
                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                }
                sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[row, colLast].VerticalAlignment = ExcelVAlign.VAlignTop;
                row++;
                int StartRow = row;
                // Get bank transaction data.

                int col = 0;
                var ledgerData = bankExtensionService.GetBankLedgerData(companyGroupId, companyId, plantId, bankMasterId, fromDate, toDate);
                if (ledgerData.Rows.Count > 0)
                {
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, Convert.ToDateTime(ledgerData.Rows[i]["PostingDate"].ToString()).ToString("dd-MMM-yyyy")); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["OtherSide"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Narration"].ToString()); col++;
                        sheet.Range[row, 4].WrapText = true;
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode == bankCurrencyCode)
                        {
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        else
                        {
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString())); col++;
                        }
                        sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(5) + row + "-" + reportUtility.GetColumnNameForXls(6) + row + ")";
                        sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        col++;
                        colLast = col;
                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyCode)
                        {
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString())); col++;
                            reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString())); col++;
                            sheet.Range[row, col].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(10) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(8) + row + "-" + reportUtility.GetColumnNameForXls(9) + row + ")"; col++;
                            sheet.Range[row, col].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            colLast = col;
                        }

                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet.Range[row, colLast].VerticalAlignment = ExcelVAlign.VAlignTop;
                        if (extended == true)
                        {
                            col++;

                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["VoucherDetailId"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ReconcileDate"].ToString()); col++;
                            reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ReconciliationStatus"].ToString());
                            colLast = col;
                        }
                        sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 2, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();
                sheet.Range[row, colDebit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colDebit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colDebit) + (row - 1) + ")";
                sheet.Range[row, colDebit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet.Range[row, colCredit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colCredit) + StartRow + ":" + reportUtility.GetColumnNameForXls(colCredit) + (row - 1) + ")";
                sheet.Range[row, colCredit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);

                sheet.Range[row, 7].Formula = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, 7].CellStyle.Font.Bold = true;
                colLast = 8;
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyCode)
                {
                    sheet.Range[row, 10].Formula = "=" + reportUtility.GetColumnNameForXls(10) + (row - 1);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 10].CellStyle.Font.Bold = true;
                    colLast = 11;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                sheet.Range[row, colLast].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[row, colLast].VerticalAlignment = ExcelVAlign.VAlignTop;
                // row++;

                sheet.Range[12, 5, row, 5].WrapText = true;
                //sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Bank Ledger", companyId, plantName, null);
                reportUtility.SetText(ref sheet, 5, 4, "From " + fromDate + " To " + toDate + "", ExcelHAlign.HAlignCenter);

                sheet.Range[reportUtility.GetColumnNameForXls(1) + 6 + ":" + reportUtility.GetColumnNameForXls(colLast) + 6].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetBankReconcileReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var colLast = 0;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                int xlsCol = 1;
                int colVoucherNo = 0;
                int colPostingDate = 0;
                //int colAccountName = 0;
                int colNarration = 0;
                int colDebit = 0;
                int colCredit = 0;
                int colBlance = 0;
                int colDrCr = 0;
                int statementNo = 0;
                int reconcileNo = 0;
                int reconcileDate = 0;

                //int colLast = xlsCol;

                // Get BankMaster data
                var bankMaster = _bankJournalService.GetBankMaster(bankMasterId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankMaster["BankName"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Branch");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 7, bankMaster["BankBranchName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(7) + row + ": " + reportUtility.GetColumnNameForXls(9) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account No");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankMaster["AccountNumber"].ToString());


                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Account Title");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 7, bankMaster["AccountTitle"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(7) + row + ": " + reportUtility.GetColumnNameForXls(9) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                var bankCurrencyId = bankMaster["CurrencyCode"].ToString();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankCurrencyId);

                reportUtility.SetMasterHeaderText(ref sheet, row, 6, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 7, bankMaster["GLGeneralInfoCode"] + " - " + bankMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(7) + row + ": " + reportUtility.GetColumnNameForXls(9) + row].Merge();

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 5, "Bank Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(7) + row + ":" + reportUtility.GetColumnNameForXls(10) + row].Merge();
                colLast = 7;

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(colLast) + row + ":" + reportUtility.GetColumnNameForXls(9) + row].Merge();
                    colLast = 9;
                }

                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Voucher No", 11); colVoucherNo = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Statement No", 10); statementNo = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Reconcile No", 10); reconcileNo = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Reconcile Date", 10); reconcileDate = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Posting Date", 10); colPostingDate = xlsCol; xlsCol++;
                //reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Account Name", 32); colAccountName = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Narration", 15); colNarration = xlsCol; xlsCol++;
                //sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

                //reportUtility.SetText(ref sheet, row, xlsCol, colNarration.) ;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 9, ExcelHAlign.HAlignRight); colDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 9, ExcelHAlign.HAlignRight); colCredit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Balance", 14, ExcelHAlign.HAlignRight); colBlance = xlsCol; xlsCol++;


                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Balance", 14, ExcelHAlign.HAlignRight);
                }
                reportUtility.SetHeaderText(ref sheet, row, 11 - 1, "Dr/Cr", ExcelHAlign.HAlignRight); colDrCr = xlsCol;

                row++;

                reportUtility.SetText(ref sheet, row, 2, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(6) + row].Merge();

                // Get bank opening balance data.
                var obVal = _bankJournalService.GetBankOpeningBalanceLedgerData(companyGroupId, companyId, plantId, bankMasterId, fromDate);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(obVal[0]["OB"]), true);
                    sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                        reportUtility.SetText(ref sheet, row, 10, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    colLast = 10;
                    sheet.Range[row, 11].Formula = "IF(" + reportUtility.GetColumnNameForXls(11 - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                }

                row++;
                // Get bank transaction data.
                var ledgerData = _bankJournalService.GetBankReconcileData(companyGroupId, companyId, plantId, bankMasterId, fromDate, toDate);
                if (ledgerData.Rows.Count > 0)
                {
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        reportUtility.SetText(ref sheet, row, 1, ledgerData.Rows[i]["VoucherNo"].ToString());
                        //sheet.Range[row, 1].WrapText = true;
                        reportUtility.SetText(ref sheet, row, 2, ledgerData.Rows[i]["BankStatementNo"].ToString());
                        reportUtility.SetText(ref sheet, row, 3, ledgerData.Rows[i]["ReconcileNo"].ToString());
                        reportUtility.SetText(ref sheet, row, 4, Convert.ToDateTime(ledgerData.Rows[i]["ReconcileDate"].ToString()).ToString("dd-MMM-yyyy"));
                        reportUtility.SetText(ref sheet, row, 5, Convert.ToDateTime(ledgerData.Rows[i]["PostingDate"].ToString()).ToString("dd-MMM-yyyy"));
                        //sheet.Range[row, 2].WrapText = true;
                        //reportUtility.SetText(ref sheet, row, 3, ledgerData.Rows[i]["OtherSide"].ToString());
                        //sheet.Range[row, 3].WrapText = true;
                        reportUtility.SetText(ref sheet, row, 6, ledgerData.Rows[i]["Narration"].ToString());
                        sheet.Range[row, 6].WrapText = true;
                        //sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

                        sheet.Range[row, 7].WrapText = true;

                        //sheet.Range[row, 4].ColumnWidth = 40;
                        //sheet.Range[row, 4].AutofitColumns();

                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode == bankCurrencyId)
                        {
                            reportUtility.SetText(ref sheet, row, colDebit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colCredit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        }
                        else
                        {
                            reportUtility.SetText(ref sheet, row, colDebit, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colCredit, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString()));
                        }
                        sheet.Range[row, 9].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(9) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colDebit) + row + "-" + reportUtility.GetColumnNameForXls(colCredit) + row + ")";
                        sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                        {
                            reportUtility.SetText(ref sheet, row, colDebit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, colCredit, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, 9].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(9) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(colDebit) + row + "-" + reportUtility.GetColumnNameForXls(colCredit) + row + ")";
                            sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        }
                        colLast = 10;
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 2, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(6) + row].Merge();

                sheet.Range[row, 6].Formula = "=" + reportUtility.GetColumnNameForXls(6) + (row - 1);
                sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, 6].CellStyle.Font.Bold = true;
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                {
                    sheet.Range[row, 10].Formula = "=" + reportUtility.GetColumnNameForXls(10) + (row - 1);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 10].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                sheet.Range[11, 4, row, 4].WrapText = true;
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Bank Reconcile Ledger", companyId, plantName, null);
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

        public IWorkbook xGetBankLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate)
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
                sheet.Name = "Ledger";

                // Get BankMaster data
                var bankMaster = _bankJournalService.GetBankMaster(bankMasterId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankMaster["BankName"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Branch");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["BankBranchName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account No");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankMaster["AccountNumber"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Account Title");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["AccountTitle"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                var bankCurrencyId = bankMaster["CurrencyCode"].ToString();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankCurrencyId);

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["GLGeneralInfoCode"] + " - " + bankMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 5, "Bank Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ":" + reportUtility.GetColumnNameForXls(7) + row].Merge();
                colLast = 8;

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(colLast) + row + ":" + reportUtility.GetColumnNameForXls(10) + row].Merge();
                    colLast = 11;
                }

                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, 1, "Voucher No", 11);
                reportUtility.SetHeaderText(ref sheet, row, 2, "Posting Date", 10);
                //reportUtility.SetHeaderText(ref sheet, row, 3, "Account Name", 32);
                reportUtility.SetHeaderText(ref sheet, row, 4, "Narration", 28);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Debit", 9, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 6, "Credit", 9, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 7, "Balance", 14, ExcelHAlign.HAlignRight);

                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 8, "Debit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 9, "Credit", 9, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 10, "Balance", 14, ExcelHAlign.HAlignRight);
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", ExcelHAlign.HAlignRight);

                row++;
                reportUtility.SetText(ref sheet, row, 2, "Opening Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

                // Get bank opening balance data.
                var obVal = _bankJournalService.GetBankOpeningBalanceLedgerData(companyGroupId, companyId, plantId, bankMasterId, fromDate);
                if (obVal.Count > 0)
                {
                    // Set Opening Balance
                    reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(obVal[0]["OB"]), true);
                    sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                    if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                        reportUtility.SetText(ref sheet, row, 10, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                }

                row++;
                // Get bank transaction data.
                var ledgerData = _bankJournalService.GetBankLedgerData(companyGroupId, companyId, plantId, bankMasterId, fromDate, toDate);
                if (ledgerData.Rows.Count > 0)
                {
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        reportUtility.SetText(ref sheet, row, 1, ledgerData.Rows[i]["VoucherNo"].ToString());
                        reportUtility.SetText(ref sheet, row, 2, Convert.ToDateTime(ledgerData.Rows[i]["PostingDate"].ToString()).ToString("dd-MMM-yyyy"));
                        //reportUtility.SetText(ref sheet, row, 3, ledgerData.Rows[i]["OtherSide"].ToString());
                        reportUtility.SetText(ref sheet, row, 4, ledgerData.Rows[i]["Narration"].ToString());
                        sheet.Range[row, 4].WrapText = true;

                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode == bankCurrencyId)
                        {
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        }
                        else
                        {
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(ledgerData.Rows[i]["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 6, Convert.ToDouble(ledgerData.Rows[i]["CrAmount"].ToString()));
                        }
                        sheet.Range[row, 7].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(7) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(5) + row + "-" + reportUtility.GetColumnNameForXls(6) + row + ")";
                        sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                        // Base currency checking
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                        {
                            reportUtility.SetText(ref sheet, row, 8, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 9, Convert.ToDouble(ledgerData.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                            sheet.Range[row, 10].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(10) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(8) + row + "-" + reportUtility.GetColumnNameForXls(9) + row + ")";
                            sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        }
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 2, "Closing Balance", true);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(4) + row].Merge();

                sheet.Range[row, 7].Formula = "=" + reportUtility.GetColumnNameForXls(7) + (row - 1);
                sheet.Range[row, 7].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                sheet.Range[row, 7].CellStyle.Font.Bold = true;
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyCode != bankCurrencyId)
                {
                    sheet.Range[row, 10].Formula = "=" + reportUtility.GetColumnNameForXls(10) + (row - 1);
                    sheet.Range[row, 10].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, 10].CellStyle.Font.Bold = true;
                }
                sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                sheet.Range[11, 4, row, 4].WrapText = true;
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Bank Ledger", companyId, plantName, null);
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
        private IWorkbook xGetBankBookReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate)
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
                sheet.Name = "Ledger";

                // Get BankMaster data
                var bankMaster = _bankJournalService.GetBankMaster(bankMasterId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, bankMaster["BankName"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Branch");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 4, bankMaster["BankBranchName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account No");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, bankMaster["AccountNumber"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 3, "Account Title");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 4, bankMaster["AccountTitle"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank Currency");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 2, bankMaster["CurrencyCode"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 3, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 4, bankMaster["GLGeneralInfoCode"] + " - " + bankMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 4, "Bank Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(6) + row].Merge();
                colLast = 7;

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode, out string companyGroupCurrencyId, out string companyGroupCurrencyCode, out string hardCurrencyId, out string hardCurrencyCode);
                var bankCurrencyId = bankMaster["CurrencyId"].ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != bankCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(colLast) + row + ":" + reportUtility.GetColumnNameForXls(9) + row].Merge();
                    colLast = 10;
                }

                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, 1, "Voucher No", 12);
                reportUtility.SetHeaderText(ref sheet, row, 2, "Account Name", 28);
                reportUtility.SetHeaderText(ref sheet, row, 3, "Narration", 28);
                reportUtility.SetHeaderText(ref sheet, row, 4, "Debit", 8, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 5, "Credit", 8, ExcelHAlign.HAlignRight);
                reportUtility.SetHeaderText(ref sheet, row, 6, "Balance", 10, ExcelHAlign.HAlignRight);

                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != bankCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 7, "Debit", 8, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 8, "Credit", 8, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 9, "Balance", 10, ExcelHAlign.HAlignRight);
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", ExcelHAlign.HAlignRight);
                row++;

                var ledgerData = _bankJournalService.GetBankLedgerData(companyGroupId, companyId, plantId, bankMasterId, fromDate, toDate);
                var obVal = _bankJournalService.GetBankOpeningBalanceLedgerData(companyGroupId, companyId, plantId, bankMasterId, fromDate);
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
                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != bankCurrencyId)
                                reportUtility.SetText(ref sheet, row, 9, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                            isOB = false;
                        }
                        else
                        {
                            reportUtility.SetFormula(ref sheet, row, 6, lastClosing, true);
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        }

                        row++;
                        for (int i = 0; i < data.Rows.Count; i++)
                        {
                            reportUtility.SetText(ref sheet, row, 1, data.Rows[i]["VoucherNo"].ToString());
                            reportUtility.SetText(ref sheet, row, 2, data.Rows[i]["OtherSide"].ToString());
                            reportUtility.SetText(ref sheet, row, 3, data.Rows[i]["Narration"].ToString());
                            reportUtility.SetText(ref sheet, row, 4, Convert.ToDouble(data.Rows[i]["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(data.Rows[i]["CrAmount"].ToString()));
                            sheet.Range[row, 6].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(6) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(4) + row + "-" + reportUtility.GetColumnNameForXls(5) + row + ")";
                            sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;

                            // Base currency checking
                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != bankCurrencyId)
                            {
                                reportUtility.SetText(ref sheet, row, 7, Convert.ToDouble(data.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                                reportUtility.SetText(ref sheet, row, 8, Convert.ToDouble(data.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                                sheet.Range[row, 9].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(9) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(7) + row + "-" + reportUtility.GetColumnNameForXls(8) + row + ")";
                                sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                sheet.Range[row, 9].VerticalAlignment = ExcelVAlign.VAlignTop;
                            }
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                            row++;
                        }
                        reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

                        sheet.Range[row, 6].Formula = "=" + reportUtility.GetColumnNameForXls(6) + (row - 1);
                        lastClosing = "=" + reportUtility.GetColumnNameForXls(6) + (row - 1);
                        sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, 6].CellStyle.Font.Bold = true;
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != bankCurrencyId)
                        {
                            sheet.Range[row, 9].Formula = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                            sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 9].CellStyle.Font.Bold = true;
                        }
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        row++;
                    }
                }

                sheet.Range[11, 4, row, 4].WrapText = true;
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Bank Book", companyId, plantName, null);
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

        public IWorkbook GetBankBookReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate)
        {
            try
            {
                AccountsBankReportService accountsBankReportService = new AccountsBankReportService(_sqlRepository);
                var row = 6;
                var colLast = row;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                // Get BankMaster data
                var bankMaster = _bankJournalService.GetBankMaster(bankMasterId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankMaster["BankName"].ToString());

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Branch");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["BankBranchName"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Account No");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankMaster["AccountNumber"].ToString());


                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Account Title");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["AccountTitle"].ToString());
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Bank Currency");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(2) + row].Merge();
                var bankCurrencyId = bankMaster["CurrencyCode"].ToString();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, bankCurrencyId);

                reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GL");
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 5, bankMaster["GLGeneralInfoCode"] + " - " + bankMaster["GLGeneralInfoName"]);
                sheet.Range[reportUtility.GetColumnNameForXls(5) + row + ": " + reportUtility.GetColumnNameForXls(8) + row].Merge();

                row++;
                reportUtility.SetHeaderText(ref sheet, row, 5, "Bank Currency", ExcelHAlign.HAlignCenter);
                sheet.Range[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(6) + row].Merge();
                colLast = 7;

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                var cashCurrencyId = bankMaster["CurrencyId"].ToString();
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 7, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(7) + row + ":" + reportUtility.GetColumnNameForXls(9) + row].Merge();
                    colLast = 10;
                }

                // Detail Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, 1, "Voucher No", 12);
                reportUtility.SetHeaderText(ref sheet, row, 2, "Account Name", 7);
                reportUtility.SetHeaderText(ref sheet, row, 3, "Narration", 15);
                reportUtility.SetHeaderText(ref sheet, row, 4, "Debit", 15, ExcelHAlign.HAlignRight); int colDebit = 4;
                reportUtility.SetHeaderText(ref sheet, row, 5, "Credit", 15, ExcelHAlign.HAlignRight); int colCredit = 5;
                reportUtility.SetHeaderText(ref sheet, row, 6, "Balance", 15, ExcelHAlign.HAlignRight);
                if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                {
                    reportUtility.SetHeaderText(ref sheet, row, 7, "Debit", 15, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 8, "Credit", 15, ExcelHAlign.HAlignRight);
                    reportUtility.SetHeaderText(ref sheet, row, 9, "Balance", 15, ExcelHAlign.HAlignRight);
                }
                reportUtility.SetHeaderText(ref sheet, row, colLast, "Dr/Cr", 4, ExcelHAlign.HAlignRight);
                row++;
                int StartRow = row;
                // Get Cash transaction data.
                var ledgerData = accountsBankReportService.GetBankBookLedgerData(companyGroupId, companyId, plantId, bankMasterId, fromDate, toDate);
                var obVal = accountsBankReportService.GetBankOpeningBalanceLedgerData(companyGroupId, companyId, plantId, bankMasterId, fromDate);
                if (ledgerData.Rows.Count > 0)
                {
                    var dt = ledgerData.AsEnumerable().OrderBy(r => Convert.ToDateTime(r["PostingDate"]))
                            .GroupBy(r => new { PostingDate = r["PostingDate"] })
                            .Select(g => g.OrderBy(r => r["PostingDate"]).First())
                            .CopyToDataTable();
                    var isOB = true;
                    var lastClosing = string.Empty; ;
                    var lastClosing2 = string.Empty; ;
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
                                ob = Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]);
                            reportUtility.SetText(ref sheet, row, 9, ob, true);
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                            isOB = false;
                        }
                        else
                        {
                            reportUtility.SetFormula(ref sheet, row, 6, lastClosing, true);
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";

                            if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                            {
                                reportUtility.SetFormula(ref sheet, row, 9, lastClosing2, true);
                            }

                        }

                        row++;


                        int StartSegmentRow = row;
                        for (int i = 0; i < data.Rows.Count; i++)
                        {
                            reportUtility.SetText(ref sheet, row, 1, data.Rows[i]["VoucherNo"].ToString());
                            reportUtility.SetText(ref sheet, row, 2, data.Rows[i]["OtherSide"].ToString());
                            reportUtility.SetText(ref sheet, row, 3, data.Rows[i]["Narration"].ToString());
                            reportUtility.SetText(ref sheet, row, 4, Convert.ToDouble(data.Rows[i]["DrAmount"].ToString()));
                            reportUtility.SetText(ref sheet, row, 5, Convert.ToDouble(data.Rows[i]["CrAmount"].ToString()));
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
                            sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                            row++;
                        }
                        reportUtility.SetText(ref sheet, row, 1, "Closing Balance", true);
                        sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();
                        sheet.Range[row, colDebit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colDebit) + StartSegmentRow + ":" + reportUtility.GetColumnNameForXls(colDebit) + (row - 1) + ")";
                        sheet.Range[row, colDebit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                        sheet.Range[row, colCredit].Formula = "SUM(" + reportUtility.GetColumnNameForXls(colCredit) + StartSegmentRow + ":" + reportUtility.GetColumnNameForXls(colCredit) + (row - 1) + ")";
                        sheet.Range[row, colCredit].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                        sheet.Range[row, 6].Formula = "=" + reportUtility.GetColumnNameForXls(6) + (row - 1);
                        lastClosing = "=" + reportUtility.GetColumnNameForXls(6) + (row - 1);
                        lastClosing2 = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                        sheet.Range[row, 6].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                        sheet.Range[row, 1, row, colLast].CellStyle.Font.Bold = true;
                        if (!string.IsNullOrEmpty(companyCurrencyId) && companyCurrencyId != cashCurrencyId)
                        {
                            sheet.Range[row, 9].Formula = "=" + reportUtility.GetColumnNameForXls(9) + (row - 1);
                            sheet.Range[row, 9].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                            sheet.Range[row, 9].CellStyle.Font.Bold = true;
                        }
                        sheet.Range[row, colLast].Formula = "IF(" + reportUtility.GetColumnNameForXls(colLast - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                        row++;
                    }
                }

                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Bank Book", companyId, plantId, plantName, null);
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

        private string BanReconcileCRSql(string BankMasterID, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT V.Id AS VoucherId
	                                         ,VD.Id AS VoucherDetailId
	                                         ,V.VoucherNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                                             ,VD.DocRefNo, VD.PartyType, VD.Narration
	                                         ,GLT.CrAmount AS Amount --[Add : BanK other Credit]
	                                         ,'' AS CheckNo
	                                         ,'' EncashmentDate
                                       FROM TRN.VoucherDetail AS VD
                                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + BankMasterID + @"' AND (ReconcileId IS NULL))
                                       AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"'  AND V.IsPark=0
                                       AND (VD.BankMasterId='" + BankMasterID + @"'  AND V.PostingDate<=CONVERT(DATE,'" + toDate + @"')) --AND V.PostingDate>='" + fromDate + @"'
                                       AND (VD.CrAmount<>0.0000)";
        }

        private string BanKSql(string BankMasterID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT BM.Id AS BankMasterId, BM.AccountTitle, BM.AccountNumber, BM.GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
                                    , BM.BudgetMasterId, BU.Code AS BudgetCode, BU.UserName AS BudgetName, BM.ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
                                    , ACT.UserName AS BankAccountTypeName, BM.BankId, BM.Code AS BankCode, B.UserName AS BankName, BM.BankBranchId, BB.Code AS BankBranchCode, BB.UserName AS BankBranchName
                                    , BM.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName, BM.EntityId
                                    FROM [MST].[BankMaster] AS BM
                                    LEFT JOIN [HKP].[GLGeneralInfo] As GL ON GL.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BUM ON BUM.Id=BM.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS BU ON BU.Id=BUM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=BM.ActivityId
                                    LEFT JOIN [HKP].[BankAccountType] AS ACT ON ACT.Id=BM.BankAccountTypeId
                                    LEFT JOIN [HKP].[Bank] AS B ON B.Id=BM.BankId
                                    LEFT JOIN [HKP].[BankBranch] AS BB ON BB.Id=BM.BankBranchId
                                    LEFT JOIN [SCS].Currency AS C ON C.Id=BM.CurrencyId   
WHERE BM.Id='"+ BankMasterID + "'";
        }
        private string BanReconcileDRSql(string BankMasterID, string fromDate, string toDate,string cutOffDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = "";
            if (Convert.ToDateTime(cutOffDate).Date == Convert.ToDateTime(fromDate).Date)
            {
                str = " AND V.[SourceType]<>'OpeningBalance' ";
            }
            else
            {
                str = " ";
            }
            return @"SELECT V.Id AS VoucherId
	                                         ,VD.Id AS VoucherDetailId
	                                         ,V.VoucherNo
	                                         ,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
	                                         ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                                             ,VD.DocRefNo, VD.PartyType, VD.Narration
	                                         ,GLT.DrAmount AS Amount --[Add : BanK other Credit]
	                                         ,'' AS CheckNo
	                                         ,'' EncashmentDate 
                                       FROM TRN.VoucherDetail AS VD
                                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + BankMasterID + @"' AND (ReconcileId IS NULL))
                                       AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.IsPark=0
                                       AND (VD.BankMasterId='" + BankMasterID + @"'  AND V.PostingDate<=CONVERT(DATE,'" + toDate + @"')) --AND V.PostingDate>='" + fromDate + @"'
                                       AND (VD.DrAmount<>0.0000)" + str;
        }
        private string BankReconcilePendingDRSql(string bankMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT  VD.Id AS VoucherDetailId  ,V.VoucherNo,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                                       ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                                       ,VD.DocRefNo, VD.PartyType, VD.Narration ,GLT.DrAmount AS Amount 
	                                   ,'' BankReconciliationUploadedDataId,'' BankRefNo,'' BankParticulars
                                       FROM TRN.VoucherDetail AS VD
                                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' )
                                       AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.IsPark=0
                                       AND VD.BankMasterId='" + bankMasterId + @"'  AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')
                                       AND VD.DrAmount<>0.0000 
                                       AND VD.Id NOT IN(select VoucherDetailId from TRN.BankReconciliationMap) 
UNION ALL
								SELECT  '' VoucherDetailId,'' VoucherNo,REPLACE(CONVERT(CHAR(11), BRUD.Addeddate, 106),' ','-') AS  VoucherDate 
								,REPLACE(CONVERT(CHAR(11), BankStatementDate, 106),' ','-') AS  PostingDate 
                                ,'' DocRefNo, '' PartyType, '' Narration
								, CrAmount AS Amount ,BRUD.Id BankReconciliationUploadedDataId, BankRefNo,BankParticulars
                                FROM TRN.BankReconciliationUploadedData  BRUD
                                INNER JOIN TRN.BankReconciliationUpload BRU ON BRU.Id=BRUD.BankReconciliationUploadId
                                WHERE BRUD.CompanyGroupId='" + identity.CompanyGroupId + "' AND BRUD.CompanyId='" + identity.CompanyId + "' AND BRUD.PlantId='" + identity.PlantId + "'  AND BRU.BankMasterId='" + bankMasterId + @"' 
                                AND BankStatementDate BETWEEN CONVERT(DATE,'" + fromDate + "') AND CONVERT(DATE,'" + toDate + @"') AND CrAmount>0 
                                AND BRUD.Id NOT IN(select BankReconciliationUploadedDataId from TRN.BankReconciliationMap)";
        }
        private string BankReconcilePendingCRSql(string bankMasterId, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT  VD.Id AS VoucherDetailId  ,V.VoucherNo,REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                                      ,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                                      ,VD.DocRefNo, VD.PartyType, VD.Narration ,GLT.CrAmount AS Amount 
	                                   ,'' BankReconciliationUploadedDataId,'' BankRefNo,'' BankParticulars
                                       FROM TRN.VoucherDetail AS VD
                                       INNER JOIN TRN.Voucher AS V ON VD.VoucherId=V.Id
                                       INNER JOIN TRN.GLTransactionDetail AS GLT ON GLT.VoucherDetailId=VD.Id
                                       WHERE VD.Id IN(SELECT VoucherDetailId FROM TRN.GLTransactionDetail WHERE BankMasterId='" + bankMasterId + @"' )
                                       AND V.CompanyGroupId='" + identity.CompanyGroupId + @"' AND V.CompanyId='" + identity.CompanyId + @"' AND V.IsPark=0
                                       AND VD.BankMasterId='" + bankMasterId + @"'  AND V.PostingDate BETWEEN CONVERT(DATE,'" + fromDate + @"') AND CONVERT(DATE,'" + toDate + @"')
                                       AND VD.CrAmount<>0.0000 
                                       AND VD.Id NOT IN(select VoucherDetailId from TRN.BankReconciliationMap) 
UNION ALL
								SELECT  '' VoucherDetailId,'' VoucherNo,REPLACE(CONVERT(CHAR(11), BRUD.Addeddate, 106),' ','-') AS  VoucherDate 
								,REPLACE(CONVERT(CHAR(11), BankStatementDate, 106),' ','-') AS  PostingDate 
                                ,'' DocRefNo, '' PartyType, '' Narration
								, DrAmount AS Amount ,BRUD.Id BankReconciliationUploadedDataId, BankRefNo,BankParticulars
                                FROM TRN.BankReconciliationUploadedData  BRUD
                                INNER JOIN TRN.BankReconciliationUpload BRU ON BRU.Id=BRUD.BankReconciliationUploadId
                                WHERE BRUD.CompanyGroupId='" + identity.CompanyGroupId + "' AND BRUD.CompanyId='" + identity.CompanyId + "' AND BRUD.PlantId='" + identity.PlantId + "'  AND BRU.BankMasterId='" + bankMasterId + @"' 
                                AND BankStatementDate BETWEEN CONVERT(DATE,'" + fromDate + "') AND CONVERT(DATE,'" + toDate + @"') AND DrAmount>0 
                                AND BRUD.Id NOT IN(select BankReconciliationUploadedDataId from TRN.BankReconciliationMap)";
        }

        public void CRReconcileReport(string BankMasterID,string fromDate,string toDate)
        {
            try
            {
                string sql = BanReconcileCRSql( BankMasterID,  fromDate, toDate);
                string Banksql = BanKSql(BankMasterID);

                //Instantiate the Excel application object
                DataTable dtBank = _sqlRepository.GetDataTable(Banksql);
                DataTable dtCRBR = _sqlRepository.GetDataTable(sql);
                if (dtCRBR.Rows.Count == 0)
                    throw new Exception("No data found");
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Cr. Reconcile Pending Report";

                int ROW = 6;
                int COL = 1;

                #region Header
         
                int StartRow = ROW;
                sheet[ROW, COL].Text = "Bank :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;

                int colBank = COL;
                ROW++;
                sheet[ROW, COL].Text = "Branch :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBranch = COL;
                ROW++;
                sheet[ROW, COL].Text = "From Date :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colFromDate = COL;
                ROW = StartRow;
                COL = 4;
                sheet[ROW, COL].Text = "Account :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colAccount = COL;
                ROW++;
                sheet[ROW, COL].Text = "Bank GL :";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBankGL = COL;
                ROW++;
                sheet[ROW, COL].Text = "To Date :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colToDate = COL;
                ROW = StartRow;
                COL = 7;
                sheet[ROW, COL].Text = "Bank Currency :";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBankCurrency = COL;
                // Headerdata
                ROW = 6;
                sheet[ROW, colBank + 1].Text = dtBank.Rows[0]["BankName"].ToString();
                ROW++;
                sheet[ROW, colBranch + 1].Text = dtBank.Rows[0]["BankBranchName"].ToString();
                ROW++;
                sheet[ROW, colFromDate + 1].Text = fromDate;
                ROW = StartRow;
                sheet[ROW, colAccount + 1].Text = dtBank.Rows[0]["AccountTitle"].ToString();
                ROW++;
               
                sheet[ROW, colBankGL + 1].Text = dtBank.Rows[0]["GLGeneralInfoId"].ToString() + "-" + dtBank.Rows[0]["GLGeneralInfoName"].ToString();

                ROW++;
                sheet[ROW, colToDate + 1].Text = toDate;
                ROW = StartRow;
                sheet[ROW, colBankCurrency + 1].Text = dtBank.Rows[0]["CurrencyCode"].ToString();

                sheet.Range[StartRow, colBank + 1, StartRow , colBank + 2].Merge();
                sheet.Range[StartRow+1, colBranch + 1, StartRow + 1, colBranch + 2].Merge();
                sheet.Range[StartRow + 2, colFromDate + 1, StartRow + 2, colFromDate + 2].Merge();
                sheet.Range[StartRow, colAccount + 1, StartRow, colAccount + 2].Merge();
                sheet.Range[StartRow+1, colBankGL + 1, StartRow + 1, colBankGL + 2].Merge();
                sheet.Range[StartRow+2, colToDate + 1, StartRow + 2, colToDate + 2].Merge();
                sheet.Range[StartRow, colBankCurrency + 1, StartRow, colBankCurrency + 2].Merge();
                sheet.Range[StartRow, colBank, StartRow+3, colBankCurrency + 2].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(232, 244, 248);
              
                ROW = 10;
                COL = 1;
                #endregion
                sheet[ROW, COL].Text = "Id";
                sheet[ROW, COL].ColumnWidth = 12;
                int colId = COL;
                COL++;
                sheet[ROW, COL].Text = "Voucher No";
                sheet[ROW, COL].ColumnWidth = 18;
                int colVoucherNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Voucher Date";
                sheet[ROW, COL].ColumnWidth = 15;
                int colVoucherDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Posting Date";
                sheet[ROW, COL].ColumnWidth = 15;
                int colPostingDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Doc Ref No.";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDocRefNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Type";
                sheet[ROW, COL].ColumnWidth = 20;
                int colPartyType = COL;
                COL++;
                sheet[ROW, COL].Text = "Narration";
                sheet[ROW, COL].ColumnWidth = 15;
                int colNarration = COL;
                COL++;
                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                int colAmount = COL;

                COL++;
                sheet[ROW, COL].Text = "Check No.";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCheckNo = COL;


                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                 StartRow = ROW; //row 20
                for (int i = 0; i < dtCRBR.Rows.Count; i++)
                {

                    sheet[ROW, colId].Text = dtCRBR.Rows[i]["VoucherDetailId"].ToString();
                    sheet[ROW, colVoucherNo].Text = dtCRBR.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, colVoucherDate].Text = dtCRBR.Rows[i]["VoucherDate"].ToString();

                    sheet[ROW, colPostingDate].Text = dtCRBR.Rows[i]["PostingDate"].ToString();
                    sheet[ROW, colDocRefNo].Text = dtCRBR.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, colNarration].Text = dtCRBR.Rows[i]["PartyType"].ToString();
                    sheet[ROW, colPartyType].Text = dtCRBR.Rows[i]["Narration"].ToString();
                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtCRBR.Rows[i]["Amount"].ToString());
                    sheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colCheckNo].Text = dtCRBR.Rows[i]["CheckNo"].ToString();


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }
                sheet[ROW, 1].Text = "Total:";
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                int colTotal = COL;
                var reportUtility = new ReportUtility();
                sheet.Range[ROW, colAmount].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colAmount) + StartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + (ROW - 1) + ")";
                sheet.Range[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet[ROW, 1].CellStyle.Font.Size = 9;

                sheet["A" + StartRow.ToString()].FreezePanes();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility.PlantHeader(ref sheet, endCol, "Cr. Reconcile Pending Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "CRReconcilePendingReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void DRReconcileReport(string BankMasterID, string fromDate, string toDate,string cutOffDate)
        {
            try
            {
                //if (string.IsNullOrEmpty(entityid) || entityid == "''")
                //    throw new Exception("Select entity");

                string sql = BanReconcileDRSql(BankMasterID, fromDate, toDate, cutOffDate);
                string Banksql = BanKSql(BankMasterID);


                //Instantiate the Excel application object
                DataTable dtBank = _sqlRepository.GetDataTable(Banksql);
                DataTable dtDRBR = _sqlRepository.GetDataTable(sql);
                if (dtDRBR.Rows.Count == 0)
                    throw new Exception("No data found");
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];
                sheet.Name = "Dr. Reconcile Pending Report";

                int ROW = 6;
                int COL = 1;

                #region Header

                int StartRow = ROW;
                sheet[ROW, COL].Text = "Bank :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;

                int colBank = COL;
                ROW++;
                sheet[ROW, COL].Text = "Branch :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBranch = COL;
                ROW++;
                sheet[ROW, COL].Text = "From Date :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colFromDate = COL;
                ROW = StartRow;
                COL = 4;
                sheet[ROW, COL].Text = "Account :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colAccount = COL;
                ROW++;
                sheet[ROW, COL].Text = "Bank GL :";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBankGL = COL;
                ROW++;
                sheet[ROW, COL].Text = "To Date :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colToDate = COL;
                ROW = StartRow;
                COL = 7;
                sheet[ROW, COL].Text = "Bank Currency :";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBankCurrency = COL;
                // Headerdata
                ROW = 6;
                sheet[ROW, colBank + 1].Text = dtBank.Rows[0]["BankName"].ToString();
                ROW++;
                sheet[ROW, colBranch + 1].Text = dtBank.Rows[0]["BankBranchName"].ToString();
                ROW++;
                sheet[ROW, colFromDate + 1].Text = fromDate;
                ROW = StartRow;
                sheet[ROW, colAccount + 1].Text = dtBank.Rows[0]["AccountTitle"].ToString();
                ROW++;
                sheet[ROW, colBankGL + 1].Text = dtBank.Rows[0]["GLGeneralInfoId"].ToString() +"-"+ dtBank.Rows[0]["GLGeneralInfoName"].ToString(); 
                 ROW++;
                sheet[ROW, colToDate + 1].Text = toDate;
                ROW = StartRow;
                sheet[ROW, colBankCurrency + 1].Text = dtBank.Rows[0]["CurrencyCode"].ToString();

                sheet.Range[StartRow, colBank + 1, StartRow, colBank + 2].Merge();
                sheet.Range[StartRow + 1, colBranch + 1, StartRow + 1, colBranch + 2].Merge();
                sheet.Range[StartRow + 2, colFromDate + 1, StartRow + 2, colFromDate + 2].Merge();
                sheet.Range[StartRow, colAccount + 1, StartRow, colAccount + 2].Merge();
                sheet.Range[StartRow + 1, colBankGL + 1, StartRow + 1, colBankGL + 2].Merge();
                sheet.Range[StartRow + 2, colToDate + 1, StartRow + 2, colToDate + 2].Merge();
                sheet.Range[StartRow, colBankCurrency + 1, StartRow, colBankCurrency + 2].Merge();
                sheet.Range[StartRow, colBank, StartRow + 3, colBankCurrency + 2].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(232, 244, 248);


                ROW = 10;
                COL = 1;
                #endregion
                sheet[ROW, COL].Text = "Id";
                sheet[ROW, COL].ColumnWidth = 12;
                int colId = COL;
                COL++;
                sheet[ROW, COL].Text = "Voucher No";
                sheet[ROW, COL].ColumnWidth = 18;
                int colVoucherNo = COL;
                COL++;
                sheet[ROW, COL].Text = "Voucher Date";
                sheet[ROW, COL].ColumnWidth = 15;
                int colVoucherDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Posting Date";
                sheet[ROW, COL].ColumnWidth = 15;
                int colPostingDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Doc Ref No.";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDocRefNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Type";
                sheet[ROW, COL].ColumnWidth = 20;
                int colPartyType = COL;
                COL++;
                sheet[ROW, COL].Text = "Narration";
                sheet[ROW, COL].ColumnWidth = 15;
                int colNarration = COL;
                COL++;
                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                int colAmount = COL;

                COL++;
                sheet[ROW, COL].Text = "Check No.";
                sheet[ROW, COL].ColumnWidth = 15;
                int colCheckNo = COL;

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                 StartRow = ROW; //row 20
                for (int i = 0; i < dtDRBR.Rows.Count; i++)
                {
                    sheet[ROW, colId].Text = dtDRBR.Rows[i]["VoucherDetailId"].ToString();

                    sheet[ROW, colVoucherNo].Text = dtDRBR.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, colVoucherDate].Text = dtDRBR.Rows[i]["VoucherDate"].ToString();

                    sheet[ROW, colPostingDate].Text = dtDRBR.Rows[i]["PostingDate"].ToString();
                    sheet[ROW, colDocRefNo].Text = dtDRBR.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, colNarration].Text = dtDRBR.Rows[i]["PartyType"].ToString();
                    sheet[ROW, colPartyType].Text = dtDRBR.Rows[i]["Narration"].ToString();
                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtDRBR.Rows[i]["Amount"].ToString());
                    sheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colCheckNo].Text = dtDRBR.Rows[i]["CheckNo"].ToString();


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }
                sheet[ROW, 1].Text = "Total:";
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                int colTotal = COL;
                var reportUtility = new ReportUtility();
                sheet.Range[ROW, colAmount].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colAmount) + StartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + (ROW - 1) + ")";
                sheet.Range[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet[ROW, 1].CellStyle.Font.Size = 9;

                sheet["A" + StartRow.ToString()].FreezePanes();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility.PlantHeader(ref sheet, endCol, "Dr. Reconcile Pending Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "DrReconcilePendingReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void DRReconcilePendingReport(string bankMasterId, string fromDate, string toDate)
        {
            try
            {
                
                string sql = BankReconcilePendingDRSql(bankMasterId, fromDate, toDate);
                string Banksql = BanKSql(bankMasterId);


                //Instantiate the Excel application object
                DataTable dtBank = _sqlRepository.GetDataTable(Banksql);
                DataTable dtDRBR = _sqlRepository.GetDataTable(sql);
                if (dtDRBR.Rows.Count == 0)
                    throw new Exception("No data found");
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];
                sheet.Name = "Dr. Reconcile Pending Report";

                int ROW = 6;
                int COL = 1;

                #region Header

                int StartRow = ROW;
                sheet[ROW, COL].Text = "Bank :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;

                int colBank = COL;
                ROW++;
                sheet[ROW, COL].Text = "Branch :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBranch = COL;
                ROW++;
                sheet[ROW, COL].Text = "From Date :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colFromDate = COL;
                ROW = StartRow;
                COL = 4;
                sheet[ROW, COL].Text = "Account :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colAccount = COL;
                ROW++;
                sheet[ROW, COL].Text = "Bank GL :";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBankGL = COL;
                ROW++;
                sheet[ROW, COL].Text = "To Date :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colToDate = COL;
                ROW = StartRow;
                COL = 7;
                sheet[ROW, COL].Text = "Bank Currency :";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBankCurrency = COL;
                // Headerdata
                ROW = 6;
                sheet[ROW, colBank + 1].Text = dtBank.Rows[0]["BankName"].ToString();
                ROW++;
                sheet[ROW, colBranch + 1].Text = dtBank.Rows[0]["BankBranchName"].ToString();
                ROW++;
                sheet[ROW, colFromDate + 1].Text = fromDate;
                ROW = StartRow;
                sheet[ROW, colAccount + 1].Text = dtBank.Rows[0]["AccountTitle"].ToString();
                ROW++;
                sheet[ROW, colBankGL + 1].Text = dtBank.Rows[0]["GLGeneralInfoId"].ToString() + "-" + dtBank.Rows[0]["GLGeneralInfoName"].ToString();
                ROW++;
                sheet[ROW, colToDate + 1].Text = toDate;
                ROW = StartRow;
                sheet[ROW, colBankCurrency + 1].Text = dtBank.Rows[0]["CurrencyCode"].ToString();

                sheet.Range[StartRow, colBank + 1, StartRow, colBank + 2].Merge();
                sheet.Range[StartRow + 1, colBranch + 1, StartRow + 1, colBranch + 2].Merge();
                sheet.Range[StartRow + 2, colFromDate + 1, StartRow + 2, colFromDate + 2].Merge();
                sheet.Range[StartRow, colAccount + 1, StartRow, colAccount + 2].Merge();
                sheet.Range[StartRow + 1, colBankGL + 1, StartRow + 1, colBankGL + 2].Merge();
                sheet.Range[StartRow + 2, colToDate + 1, StartRow + 2, colToDate + 2].Merge();
                sheet.Range[StartRow, colBankCurrency + 1, StartRow, colBankCurrency + 2].Merge();
                sheet.Range[StartRow, colBank, StartRow + 3, colBankCurrency + 2].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(232, 244, 248);


                ROW = 10;
                COL = 1;
                #endregion
                sheet[ROW, COL].Text = "Entry Date";
                sheet[ROW, COL].ColumnWidth = 15;
                int colVoucherDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Posting Date";
                sheet[ROW, COL].ColumnWidth = 15;
                int colPostingDate = COL;
                COL++;
                sheet[ROW, COL].Text = "VoucherRowId";
                sheet[ROW, COL].ColumnWidth = 15;
                int colId = COL;
                COL++;
                sheet[ROW, COL].Text = "BankRowId";
                sheet[ROW, COL].ColumnWidth = 15;
                int colBankReconciliationUploadedDataId = COL;
                COL++;

                sheet[ROW, COL].Text = "Doc Ref No.";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDocRefNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Type";
                sheet[ROW, COL].ColumnWidth = 20;
                int colPartyType = COL;
                COL++;
                sheet[ROW, COL].Text = "Narration";
                sheet[ROW, COL].ColumnWidth = 15;
                int colNarration = COL;
                COL++;
                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                int colAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "MissingInGL";
                sheet[ROW, COL].ColumnWidth = 15;
                int colBankMissing = COL;
                COL++;

                sheet[ROW, COL].Text = "MissingInBank";
                sheet[ROW, COL].ColumnWidth = 15;
                int colGLMissing = COL;
                COL++;

                sheet[ROW, COL].Text = "BankParticulars";
                sheet[ROW, COL].ColumnWidth = 15;
                int colBankParticulars = COL;

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                StartRow = ROW; //row 20
                for (int i = 0; i < dtDRBR.Rows.Count; i++)
                {
                    sheet[ROW, colVoucherDate].Text = dtDRBR.Rows[i]["VoucherDate"].ToString();
                    sheet[ROW, colPostingDate].Text = dtDRBR.Rows[i]["PostingDate"].ToString();
                    sheet[ROW, colId].Text = dtDRBR.Rows[i]["VoucherDetailId"].ToString();
                    sheet[ROW, colBankReconciliationUploadedDataId].Text = dtDRBR.Rows[i]["BankReconciliationUploadedDataId"].ToString();
                    sheet[ROW, colDocRefNo].Text = dtDRBR.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, colNarration].Text = dtDRBR.Rows[i]["PartyType"].ToString();
                    sheet[ROW, colPartyType].Text = dtDRBR.Rows[i]["Narration"].ToString();
                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtDRBR.Rows[i]["Amount"].ToString());
                    sheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colBankMissing].Text = dtDRBR.Rows[i]["BankRefNo"].ToString();
                    sheet[ROW, colGLMissing].Text = dtDRBR.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, colBankParticulars].Text = dtDRBR.Rows[i]["BankParticulars"].ToString();


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }
                sheet[ROW, 1].Text = "Total:";
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                int colTotal = COL;
                var reportUtility = new ReportUtility();
                sheet.Range[ROW, colAmount].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colAmount) + StartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + (ROW - 1) + ")";
                sheet.Range[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet[ROW, 1].CellStyle.Font.Size = 9;

                sheet["A" + StartRow.ToString()].FreezePanes();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility.PlantHeader(ref sheet, endCol, "Dr. Reconcile Pending Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "DrReconcilePendingReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void CRReconcilePendingReport(string bankMasterId, string fromDate, string toDate)
        {
            try
            {

                string sql = BankReconcilePendingCRSql(bankMasterId, fromDate, toDate);
                string Banksql = BanKSql(bankMasterId);


                //Instantiate the Excel application object
                DataTable dtBank = _sqlRepository.GetDataTable(Banksql);
                DataTable dtDRBR = _sqlRepository.GetDataTable(sql);
                if (dtDRBR.Rows.Count == 0)
                    throw new Exception("No data found");
                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];
                sheet.Name = "Cr. Reconcile Pending Report";

                int ROW = 6;
                int COL = 1;

                #region Header

                int StartRow = ROW;
                sheet[ROW, COL].Text = "Bank :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;

                int colBank = COL;
                ROW++;
                sheet[ROW, COL].Text = "Branch :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBranch = COL;
                ROW++;
                sheet[ROW, COL].Text = "From Date :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colFromDate = COL;
                ROW = StartRow;
                COL = 4;
                sheet[ROW, COL].Text = "Account :";
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                sheet[ROW, COL].ColumnWidth = 10;
                int colAccount = COL;
                ROW++;
                sheet[ROW, COL].Text = "Bank GL :";
                sheet[ROW, COL].ColumnWidth = 10;
                int colBankGL = COL;
                ROW++;
                sheet[ROW, COL].Text = "To Date :";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colToDate = COL;
                ROW = StartRow;
                COL = 7;
                sheet[ROW, COL].Text = "Bank Currency :";
                sheet[ROW, COL].ColumnWidth = 13;
                sheet[ROW, COL].CellStyle.Font.Bold = true;
                int colBankCurrency = COL;
                // Headerdata
                ROW = 6;
                sheet[ROW, colBank + 1].Text = dtBank.Rows[0]["BankName"].ToString();
                ROW++;
                sheet[ROW, colBranch + 1].Text = dtBank.Rows[0]["BankBranchName"].ToString();
                ROW++;
                sheet[ROW, colFromDate + 1].Text = fromDate;
                ROW = StartRow;
                sheet[ROW, colAccount + 1].Text = dtBank.Rows[0]["AccountTitle"].ToString();
                ROW++;
                sheet[ROW, colBankGL + 1].Text = dtBank.Rows[0]["GLGeneralInfoId"].ToString() + "-" + dtBank.Rows[0]["GLGeneralInfoName"].ToString();
                ROW++;
                sheet[ROW, colToDate + 1].Text = toDate;
                ROW = StartRow;
                sheet[ROW, colBankCurrency + 1].Text = dtBank.Rows[0]["CurrencyCode"].ToString();

                sheet.Range[StartRow, colBank + 1, StartRow, colBank + 2].Merge();
                sheet.Range[StartRow + 1, colBranch + 1, StartRow + 1, colBranch + 2].Merge();
                sheet.Range[StartRow + 2, colFromDate + 1, StartRow + 2, colFromDate + 2].Merge();
                sheet.Range[StartRow, colAccount + 1, StartRow, colAccount + 2].Merge();
                sheet.Range[StartRow + 1, colBankGL + 1, StartRow + 1, colBankGL + 2].Merge();
                sheet.Range[StartRow + 2, colToDate + 1, StartRow + 2, colToDate + 2].Merge();
                sheet.Range[StartRow, colBankCurrency + 1, StartRow, colBankCurrency + 2].Merge();
                sheet.Range[StartRow, colBank, StartRow + 3, colBankCurrency + 2].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(232, 244, 248);


                ROW = 10;
                COL = 1;
                #endregion
                sheet[ROW, COL].Text = "Entry Date";
                sheet[ROW, COL].ColumnWidth = 15;
                int colVoucherDate = COL;
                COL++;
                sheet[ROW, COL].Text = "Posting Date";
                sheet[ROW, COL].ColumnWidth = 15;
                int colPostingDate = COL;
                COL++;
                sheet[ROW, COL].Text = "VoucherRowId";
                sheet[ROW, COL].ColumnWidth = 15;
                int colId = COL;
                COL++;
                sheet[ROW, COL].Text = "BankRowId";
                sheet[ROW, COL].ColumnWidth = 15;
                int colBankReconciliationUploadedDataId = COL;
                COL++;

                sheet[ROW, COL].Text = "Doc Ref No.";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDocRefNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Type";
                sheet[ROW, COL].ColumnWidth = 20;
                int colPartyType = COL;
                COL++;
                sheet[ROW, COL].Text = "Narration";
                sheet[ROW, COL].ColumnWidth = 15;
                int colNarration = COL;
                COL++;
                sheet[ROW, COL].Text = "Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                int colAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "MissingInGL";
                sheet[ROW, COL].ColumnWidth = 15;
                int colBankMissing = COL;
                COL++;

                sheet[ROW, COL].Text = "MissingInBank";
                sheet[ROW, COL].ColumnWidth = 15;
                int colGLMissing = COL;
                COL++;

                sheet[ROW, COL].Text = "BankParticulars";
                sheet[ROW, COL].ColumnWidth = 15;
                int colBankParticulars = COL;

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                StartRow = ROW; //row 20
                for (int i = 0; i < dtDRBR.Rows.Count; i++)
                {
                    sheet[ROW, colVoucherDate].Text = dtDRBR.Rows[i]["VoucherDate"].ToString();
                    sheet[ROW, colPostingDate].Text = dtDRBR.Rows[i]["PostingDate"].ToString();
                    sheet[ROW, colId].Text = dtDRBR.Rows[i]["VoucherDetailId"].ToString();
                    sheet[ROW, colBankReconciliationUploadedDataId].Text = dtDRBR.Rows[i]["BankReconciliationUploadedDataId"].ToString();
                    sheet[ROW, colDocRefNo].Text = dtDRBR.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, colNarration].Text = dtDRBR.Rows[i]["PartyType"].ToString();
                    sheet[ROW, colPartyType].Text = dtDRBR.Rows[i]["Narration"].ToString();
                    sheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtDRBR.Rows[i]["Amount"].ToString());
                    sheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colBankMissing].Text = dtDRBR.Rows[i]["BankRefNo"].ToString();
                    sheet[ROW, colGLMissing].Text = dtDRBR.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, colBankParticulars].Text = dtDRBR.Rows[i]["BankParticulars"].ToString();


                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }
                sheet[ROW, 1].Text = "Total:";
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                int colTotal = COL;
                var reportUtility = new ReportUtility();
                sheet.Range[ROW, colAmount].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colAmount) + StartRow + ":" + reportUtility.GetColumnNameForXls(colAmount) + (ROW - 1) + ")";
                sheet.Range[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat(2);

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet[ROW, 1].CellStyle.Font.Size = 9;

                sheet["A" + StartRow.ToString()].FreezePanes();


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                reportUtility.PlantHeader(ref sheet, endCol, "Cr. Reconcile Pending Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "CrReconcilePendingReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


    }
}