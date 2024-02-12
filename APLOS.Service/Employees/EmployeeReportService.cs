using Library.Core;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Payments;
using Library.Service.Banks;
using Library.Service.Currencies;
using Library.Service.Expenses;
using Library.Service.FixedAssets;
using Library.Service.Helpers;
using Library.Service.Organizations;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Employees
{
    public class EmployeeReportService : IEmployeeReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IEmployeePayableService _employeePayableService;
        private readonly ICompanyService _companyService;
        private readonly IExpenseBookingService _expenseBookingService;
        private readonly IFixedAssetRegisterService _fixedAssetRegisterService;
        private readonly ICashJournalService _cashJournalService;
        private readonly IPlantService _plantService;
        public EmployeeReportService(
            ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IEmployeeInformationService employeeInformationService
            , IEmployeePayableService employeePayableService
            , ICompanyService companyService
            , IExpenseBookingService expenseBookingService
            , ICashJournalService cashJournalService
            , IFixedAssetRegisterService fixedAssetRegisterService
            , IPlantService plantService)
        {
            _sqlRepository = sqlRepository;
            _employeeInformationService = employeeInformationService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _employeePayableService = employeePayableService;
            _companyService = companyService;
            _expenseBookingService = expenseBookingService;
            _cashJournalService = cashJournalService;
            _fixedAssetRegisterService = fixedAssetRegisterService;
            _plantService = plantService;
        }

        public IWorkbook GetEmployeeOpeningBalanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId)
        {
            try
            {
                var row = 6;
                var col = 1;
                var shet2EndxlsCol = 1;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Ledger";

                var fiscalYear = _sqlRepository.GetData("SELECT FiscalYearCode, FiscalYearName, StartDate, EndDate FROM [SCS].[FiscalYear] WHERE Id='" + fiscalYearId + "'");
                var partyLedgerData = GetEmployeeOpeningBalanceLedger(companyGroupId, companyId, plantId, fiscalYearId);
                if (partyLedgerData.Rows.Count > 0)
                {
                    // Set PartyName
                    reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Party");
                    sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
                    reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, partyLedgerData.Rows[0]["PartyId"] + " - " + partyLedgerData.Rows[0]["Party"]);
                    sheet.Range[reportUtility.GetColumnNameForXls(3) + row + ": " + reportUtility.GetColumnNameForXls(5) + row].Merge();

                    row += 2;

                    // Detail Header
                    col = 1;
                    var cGL = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "GL", 24);
#pragma warning disable CS0219 // The variable 'cPartyPlant' is assigned but its value is never used
                    var cPartyPlant = 0;
#pragma warning restore CS0219 // The variable 'cPartyPlant' is assigned but its value is never used
                    col += 1;
                    var cPostingDate = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Posting Date", 10);
                    col = col + 1;
                    var cVoucherNo = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Voucher No", 12);
                    col = col + 1;
                    var cVoucherDate = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Voucher Date", 10);
                    col = col + 1;
                    var cDocRefNo = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", 12);
                    col = col + 1;
                    var cDocDate = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 10);
                    col = col + 1;
                    var cCurrency = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Currency", 7);
                    col = col + 1;
                    var cDebit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 10, ExcelHAlign.HAlignRight);
                    col = col + 1;
                    var cCredit = col;
                    reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 10, ExcelHAlign.HAlignRight);
                    col = col + 1;
                    for (int n = 0; n < 1; n++)
                    {
                        reportUtility.SetHeaderText(ref sheet, row - 1, col, partyLedgerData.Rows[n]["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                        sheet[row - 1, col, row - 1, col + 3].Merge();
                        reportUtility.SetHeaderText(ref sheet, row, col, "Debit", 10, ExcelHAlign.HAlignRight); col++;
                        reportUtility.SetHeaderText(ref sheet, row, col, "Credit", 10, ExcelHAlign.HAlignRight); col++;
                        reportUtility.SetHeaderText(ref sheet, row, col, "Balance", ExcelHAlign.HAlignRight); col++;
                        reportUtility.SetHeaderText(ref sheet, row, col, "Dr/Cr", 4, ExcelHAlign.HAlignRight); col++;
                    }
                    shet2EndxlsCol = col - 1;
                    row += 1;
                    var Row_Total_Start = row;
                    var CurrStartRow = row;
                    for (int icount = 0; icount < partyLedgerData.Rows.Count; icount++)
                    {
                        var accountCodeId = partyLedgerData.Rows[icount]["AccountCode"].ToString();
                        reportUtility.SetText(ref sheet, row, cGL, accountCodeId + " - " + partyLedgerData.Rows[icount]["GL"]);
                        reportUtility.SetText(ref sheet, row, cPostingDate, partyLedgerData.Rows[icount]["PostingDate"].ToString());
                        reportUtility.SetText(ref sheet, row, cVoucherNo, partyLedgerData.Rows[icount]["VoucherNo"].ToString());
                        reportUtility.SetText(ref sheet, row, cVoucherDate, partyLedgerData.Rows[icount]["VoucherDate"].ToString());
                        reportUtility.SetText(ref sheet, row, cDocRefNo, partyLedgerData.Rows[icount]["dDocRefNo"].ToString());
                        reportUtility.SetText(ref sheet, row, cDocDate, partyLedgerData.Rows[icount]["dDocDate"].ToString());
                        reportUtility.SetText(ref sheet, row, cCurrency, partyLedgerData.Rows[icount]["CurrencyCode"].ToString());
                        reportUtility.SetText(ref sheet, row, cDebit, Convert.ToDouble(partyLedgerData.Rows[icount]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, cCredit, Convert.ToDouble(partyLedgerData.Rows[icount]["CrAmount"].ToString()));
                        row += 1;
                    }
                    var drcrCol = cCredit + 1;
                    for (int p = 0; p < 1; p++)
                    {
                        row = CurrStartRow;
                        var drc = drcrCol++;
                        var crc = drcrCol++;
                        var blc = drcrCol++;
                        var fscount = 0;
                        var parallelCurrencyId = partyLedgerData.Rows[p]["ParallelCurrencyId"].ToString();
                        var dvDrCr = new DataView(partyLedgerData);
                        for (int icount = 0; icount < partyLedgerData.Rows.Count; icount++)
                        {
                            var voucherDetailId = partyLedgerData.Rows[icount]["VoucherDetailId"].ToString();
                            var voucherNoId = partyLedgerData.Rows[icount]["VoucherNo"].ToString();
                            dvDrCr.RowFilter = "ParallelCurrencyId='" + parallelCurrencyId + "' AND VoucherDetailId='" + voucherDetailId + "' AND VoucherNo='" + voucherNoId + "' ";
                            var dtDrCr = dvDrCr.ToTable();
                            if (dtDrCr.Rows.Count > 0)
                            {
                                reportUtility.SetText(ref sheet, row, drc, Convert.ToDouble(dtDrCr.Rows[0]["DrAmountPC"].ToString()));
                                reportUtility.SetText(ref sheet, row, crc, Convert.ToDouble(dtDrCr.Rows[0]["CrAmountPC"].ToString()));
                                if (fscount == 0)
                                {
                                    sheet.Range[row, blc].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(drc) + row + "-" + reportUtility.GetColumnNameForXls(crc) + row + ")";
                                }
                                else
                                {
                                    sheet.Range[row, blc].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(blc) + (row - 1) + "+" + reportUtility.GetColumnNameForXls(drc) + row + "-" + reportUtility.GetColumnNameForXls(crc) + row + ")";
                                }
                                sheet.Range[row, blc].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                                var formula = "IF(" + reportUtility.GetColumnNameForXls(blc) + row + ">= 0, \"Dr\", \"Cr\")";
                                sheet.Range[row, blc + 1].Formula = formula;
                                fscount++;
                            }
                            row += 1;
                        }
                    }
                    row = row + 6;
                    sheet.UsedRange.AutofitColumns();
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                    reportUtility.CompanyPlantHeader(ref sheet, shet2EndxlsCol, "Employee Opening Balance Ledger", companyId, plantName, null);
                    reportUtility.SetText(ref sheet, 4, shet2EndxlsCol, "Fiscal Year " + fiscalYear["FiscalYearName"], ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(1) + 4 + ":" + reportUtility.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                }
                else
                {
                    reportUtility.CompanyHeader(ref sheet, shet2EndxlsCol, "Employee Opening Balance Ledger", companyId);
                    reportUtility.SetText(ref sheet, 4, shet2EndxlsCol, "Fiscal Year " + fiscalYear["FiscalYearName"], ExcelHAlign.HAlignCenter);
                    sheet.Range[reportUtility.GetColumnNameForXls(1) + 4 + ":" + reportUtility.GetColumnNameForXls(shet2EndxlsCol) + 4].Merge();
                    reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                }
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private DataTable GetEmployeeOpeningBalanceLedger(string companyGroupId, string companyId, string plantId, string fiscalYearId)
        {
            var cmdText = @"SELECT V.Id, VD.Id AS VoucherDetailId, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), v.PostingDate, 106), ' ', '-') AS PostingDate
                            , V.Narration, V.PostingDate AS PostingDateSort, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), v.DocDate, 106), ' ', '-') AS DocDate, V.CurrencyId, VD.Narration dNarration, VD.DocRefNo AS dDocRefNo
                            , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') dDocDate, ISNULL(VD.DrAmount,0) DrAmount, ISNULL(VD.CrAmount,0) CrAmount, VDC.ParallelCurrencyId, ISNULL(VDC.DrAmount,0) AS DrAmountPC
                            , ISNULL(VDC.CrAmount,0) AS CrAmountPC, GLGI.AccountCode, GLGI.[Description], TC.Code AS TrnCurrency, PC.Code AS CurrencyCode, GLGI.AccountCode GLGeneralInfoCode, GLGI.UserName GL
                            , VD.GLGeneralInfoId, VD.PartyId, p.UserName AS Party, ACT.BalanceType, VD.AddedDate, VD.PartyPlantId, pp.UserName AS PartyPlant
                            , [V_Type]=CASE WHEN V.SourceType='OpeningBalance' THEN 'Yes' ELSE 'No' END
                            FROM [TRN].[Voucher] AS V
                            LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.VoucherId = v.Id
                            LEFT JOIN [HKP].[GLGeneralInfo] GLGI ON GLGI.Id=VD.GLGeneralInfoId
                            LEFT JOIN [TRN].[VoucherDetailCurrency]  AS VDC ON VDC.VoucherDetailId =VD.Id
                            LEFT JOIN [SCS].[Currency] AS PC ON PC.Id=VDC.ParallelCurrencyId
                            LEFT JOIN [SCS].[Currency] AS TC ON TC.Id=V.CurrencyId
                            LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                            LEFT JOIN [HKP].[AccountType] AS ACT on ACT.Id=AG.AccountTypeId
                            LEFT JOIN [HKP].[GLAccountType] AS AT ON AT.GLGeneralInfoId=GLGI.Id
                            LEFT JOIN [HKP].[Party] P ON P.Id=VD.PartyId
                            LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=VD.PartyPlantId
                            WHERE V.Archive=0 AND V.IsPark=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND VD.EmployeeId IS NOT NULL AND V.FiscalYearId='" + fiscalYearId + @"'
                            AND V.SourceType='OpeningBalance' ORDER BY 7 ASC";
            return _sqlRepository.GetDataTable(cmdText);
        }



        public IWorkbook GetEmployeeExpenseBookingReport(string companyGroupId, string companyId, string plantId, string plantName, string employeeId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Expense";
                var colLast = 7;
                var colLast1 = 7;
                var col = 1;
                // Get Employee Master
                var employee = _employeeInformationService.Find(employeeId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Employee");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, employee.EmployeeCode + " - " + employee.EmployeeName);
                sheet.Range[row, 3, row, 5].Merge();

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    //sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                }
                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 22); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Budget", 22); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Activity", 22); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Invoice No", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 20); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Amount", 10, ExcelHAlign.HAlignRight); col++;
                row++;

                // reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                //sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();

                // Get Employee opening balance data.
                //var obVal = GetEmployeeOpeningBalance(companyGroupId, companyId, plantId, employeeId, fromDate);
                //if (obVal.Count > 0)
                //{
                //    // Set Opening Balance
                //    if (!string.IsNullOrEmpty(companyCurrencyId))
                //        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                //    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                //}
                //row++;

                var ledgerData = _expenseBookingService.GetEmployeeExpenseBookingData(companyGroupId, companyId, plantId, employeeId, fromDate, toDate);
                // Get bank transaction data.
                double totalAmount = 0;
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["BudgetName"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ActivityName"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["InvoiceNumber"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Remarks"].ToString()); col++;
                        var amount = Convert.ToDouble(ledgerData.Rows[i]["Amount"].ToString());
                        totalAmount += amount;
                        reportUtility.SetText(ref sheet, row, col, amount); col++;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Total Amount", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(totalAmount), true);
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, col, "Employee Expense", companyId, plantId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetAssetRegisterExpenseBookingReport(string companyGroupId, string companyId, string plantId, string plantName, string fixedAssetRegisterId, string fromDate, string toDate)
        {
            try
            {
                var row = 6;
                var excelEngine = new ExcelEngine();
                var reportUtility = new ReportUtility();
                var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2016;
                var sheet = workbook.Worksheets[0];
                sheet.Name = "Expense";
                var colLast = 7;
                var colLast1 = 7;
                var col = 1;
                // Get Employee Master
                var asset = _fixedAssetRegisterService.Find(fixedAssetRegisterId);

                // Set Header
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Asset");
                sheet.Range[row, 1, row, 2].Merge();
                reportUtility.SetMiddleAlignmentText(ref sheet, row, 3, asset.SerialNo);
                sheet.Range[row, 3, row, 5].Merge();

                _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);
                if (!string.IsNullOrEmpty(companyCurrencyId))
                {
                    reportUtility.SetHeaderText(ref sheet, row, colLast + 1, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                    //sheet.Range[row, colLast + 1, row, colLast + 3].Merge();
                }
                // Set Row Header
                row++;
                reportUtility.SetHeaderText(ref sheet, row, col, "GL", 22); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Budget", 22); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Activity", 22); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Invoice No", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Ref", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Doc Date", 12); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Narration", 20); col++;
                reportUtility.SetHeaderText(ref sheet, row, col, "Amount", 10, ExcelHAlign.HAlignRight); col++;
                row++;

                // reportUtility.SetText(ref sheet, row, 1, "Opening Balance", true);
                //sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();

                // Get Employee opening balance data.
                //var obVal = GetEmployeeOpeningBalance(companyGroupId, companyId, plantId, employeeId, fromDate);
                //if (obVal.Count > 0)
                //{
                //    // Set Opening Balance
                //    if (!string.IsNullOrEmpty(companyCurrencyId))
                //        reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(obVal[0]["CompanyCurrencyOB"]), true);
                //    sheet.Range[row, col].Formula = "IF(" + reportUtility.GetColumnNameForXls(col - 1) + row + ">= 0, \"Dr\", \"Cr\")";
                //}
                //row++;

                var ledgerData = _expenseBookingService.GetAssetRegisterExpenseBookingData(companyGroupId, companyId, plantId, fixedAssetRegisterId, fromDate, toDate);
                // Get bank transaction data.
                double totalAmount = 0;
                if (ledgerData.Rows.Count > 0)
                {
                    col = 1;
                    for (int i = 0; i < ledgerData.Rows.Count; i++)
                    {
                        col = 1;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["GLGeneralInfoCode"] + " - " + ledgerData.Rows[i]["GLGeneralInfoName"]); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["BudgetName"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["ActivityName"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["InvoiceNumber"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocRefNo"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["DocDate"].ToString()); col++;
                        reportUtility.SetText(ref sheet, row, col, ledgerData.Rows[i]["Remarks"].ToString()); col++;
                        var amount = Convert.ToDouble(ledgerData.Rows[i]["Amount"].ToString());
                        totalAmount += amount;
                        reportUtility.SetText(ref sheet, row, col, amount); col++;
                        row++;
                    }
                }

                reportUtility.SetText(ref sheet, row, 1, "Total Amount", true);
                sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ":" + reportUtility.GetColumnNameForXls(colLast1) + row].Merge();
                reportUtility.SetText(ref sheet, row, col - 1, Convert.ToDouble(totalAmount), true);
                sheet.UsedRange.AutofitColumns();
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, col, "Asset Expense", companyId, plantName, "From " + fromDate + " To " + toDate + "");
                sheet.Range[reportUtility.GetColumnNameForXls(1) + 5 + ":" + reportUtility.GetColumnNameForXls(col) + 5].Merge();
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }




        public IWorkbook GetEmployeePayableExpenseBookingReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var headerData = _employeePayableService.GetEmployeePayableExpenseBookingReportHeader(companyGroupId, companyId, plantId, voucherId, SourceType.EmployeePayable);

            // Set report Name
            reportFileName = Convert.ToDateTime(headerData["PostingDate"]).ToString("yyMMdd") + " " + headerData["VoucherNo"];

            var _row = 5;
            var shet2EndxlsCol = 1;

            report.SetMasterHeaderText(ref sheet, _row, 1, "Voucher No");
            report.SetText(ref sheet, _row, 2, headerData["VoucherNo"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            report.SetMasterHeaderText(ref sheet, _row, 1, "Doc Date");
            report.SetText(ref sheet, _row, 2, headerData["DocDate"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            report.SetMasterHeaderText(ref sheet, _row, 1, "Posting Date");
            report.SetText(ref sheet, _row, 2, headerData["PostingDate"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;

            if (headerData["BeneficiaryType"].ToString() == BeneficiaryType.Self.ToString())
            {
                report.SetMasterHeaderText(ref sheet, _row, 1, "Employee (Beneficiary)");
                report.SetText(ref sheet, _row, 2, headerData["EmployeeName"].ToString());
                sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
                _row++;
            }
            else
            {
                report.SetMasterHeaderText(ref sheet, _row, 1, "Employee");
                report.SetText(ref sheet, _row, 2, headerData["EmployeeName"].ToString());
                sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
                _row++;
            }

            var _rowL = 11;
            if (headerData["BeneficiaryType"].ToString() == BeneficiaryType.Vendor.ToString())
            {
                report.SetMasterHeaderText(ref sheet, _row, 1, "Vendor (Beneficiary)");
                report.SetText(ref sheet, _row, 2, headerData["PartyName"].ToString());
                sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
                _row++;
                _rowL = 12;
            }
            else if (!string.IsNullOrEmpty(headerData["PartyName"].ToString()))
            {
                report.SetMasterHeaderText(ref sheet, _row, 1, "Vendor");
                report.SetText(ref sheet, _row, 2, headerData["PartyName"].ToString());
                sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
                _row++;
                _rowL = 12;
            }

            report.SetMasterHeaderText(ref sheet, _row, 1, "Narration");
            report.SetText(ref sheet, _row, 2, headerData["Narration"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            var _rowR = 5;

            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Entry Date");
            report.SetText(ref sheet, _rowR, 5, headerData["VoucherDate"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();
            _rowR++;

            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Invoice No");
            report.SetText(ref sheet, _rowR, 5, headerData["DocRefNo"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Fiscal Year");
            report.SetText(ref sheet, _rowR, 5, headerData["FiscalYearName"] + "(" + headerData["PeriodNo"] + ")");
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Approved By");
            report.SetText(ref sheet, _rowR, 5, headerData["ApprovedByName"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Status");
            report.SetText(ref sheet, _rowR, 5, Convert.ToBoolean(headerData["IsPark"]) ? "Parked" : "Posted");
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            var headreColIndex = 1;

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 24);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 20);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 18);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Particulars", 18);
            headreColIndex++;

            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Currency", 10);
                headreColIndex++;
                report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Trn Value", 10);
                headreColIndex++;
            }

            double _Total_Amount = 0;
            report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, companyCurrencyCode, 12, ExcelHAlign.HAlignCenter);
            sheet[_rowL - 1, headreColIndex, _rowL - 1, headreColIndex + 1].Merge();

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Debit", ExcelHAlign.HAlignRight); 
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Credit", ExcelHAlign.HAlignRight);

            shet2EndxlsCol = headreColIndex;

            double vAmount = 0;
            var data = _employeePayableService.GetExoenseBookingReportData(companyId, voucherId);

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < data.Count; n++)
            {
                _rowL++;
                var drcrCol = 1;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["GLGeneralInfoCode"] + " - " + data[n]["GLGeneralInfoName"]); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["BudgetName"].ToString()); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["ActivityName"].ToString()); drcrCol++;
                if (!string.IsNullOrEmpty(data[n]["InvoiceNo"].ToString()))
                {
                    report.SetText(ref sheet, _rowL, drcrCol, data[n]["AssetItem"] + " - " + data[n]["InvoiceNo"]); drcrCol++;
                }
                else
                {
                    report.SetText(ref sheet, _rowL, drcrCol, data[n]["AssetItem"].ToString()); drcrCol++;
                }
                if (companyCurrencyId != headerData["CurrencyId"].ToString())
                {
                    report.SetText(ref sheet, _rowL, drcrCol, data[n]["DrAmount"].ToString()); drcrCol++;
                    report.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(data[n]["CrAmount"])); drcrCol++;
                    vAmount += Convert.ToDouble(data[n]["CrAmount"].ToString());
                }

                report.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(data[n]["CompanyCurrencyDrAmount"].ToString())); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, Convert.ToDouble(data[n]["CompanyCurrencyCrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(data[n]["CompanyCurrencyCrAmount"].ToString());
            }

            _rowL++;
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetText(ref sheet, _rowL, 1, "Total :", true);
                sheet[_rowL, 1, _rowL, shet2EndxlsCol - 3].Merge();
            }
            else
            {
                report.SetText(ref sheet, _rowL, 1, "Total :", true);
                sheet[_rowL, 1, _rowL, shet2EndxlsCol - 2].Merge();
            }

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                sheet.Range[_rowL, shet2EndxlsCol - 2].Formula = "=SUM(" + report.GetColumnNameForXls(shet2EndxlsCol - 2) + Row_Total_Start + ":" + report.GetColumnNameForXls(shet2EndxlsCol - 2) + (_rowL - 1) + ")";
                sheet.Range[_rowL, shet2EndxlsCol - 2].NumberFormat = report.NumberFormatDecimalTwo();
                sheet.Range[_rowL, shet2EndxlsCol - 2].CellStyle.Font.Bold = true;
                sheet.Range[_rowL, shet2EndxlsCol - 2].BorderAround(ExcelLineStyle.Hair);
            }
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

            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                report.SetText(ref sheet, _rowL, 1, "In Word:", true);
            }

            report.SetText(ref sheet, _rowL, 1, "In Word:", true);
            if (companyCurrencyId != headerData["CurrencyId"].ToString())
            {
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(vAmount, headerData["CurrencyId"].ToString());
                sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;
                _rowL += 1;
            }
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(_Total_Amount, companyCurrencyId);
            sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;

            _rowL = _rowL + 4;

            report.SetSignatureText(ref sheet, _rowL - 1, 1, headerData["AddedBy"].ToString());
            sheet.Range[_rowL, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 1, "Prepared By", true);

            report.SetSignatureText(ref sheet, _rowL - 1, 3, headerData["CheckedBy"].ToString());
            sheet.Range[_rowL, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 3, "Checked By", true);

            report.SetSignatureText(ref sheet, _rowL - 1, 5, headerData["PostedBy"].ToString());
            sheet.Range[_rowL - 1, 5].RowHeight = 25;
            sheet.Range[_rowL, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, 5, "Posted By", true);

            sheet.Range[_rowL, shet2EndxlsCol + 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            report.SetTextMiddle(ref sheet, _rowL, shet2EndxlsCol + 1, "Authorized By", true);

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, headerData["VoucherTypeName"].ToString(), companyId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            return workbook;
        }

        public IWorkbook GetExpensesBookingReport(string companyGroupId, string companyId, string plantId, string plantName, string expenseBookingId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var headerData = _expenseBookingService.GetExpenseBookingReportHeader(companyGroupId, companyId, plantId, expenseBookingId);

            var _row = 5;
            var shet2EndxlsCol = 1;

            report.SetMasterHeaderText(ref sheet, _row, 1, "InvoiceNumber");
            report.SetText(ref sheet, _row, 2, headerData["InvoiceNumber"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;
            report.SetMasterHeaderText(ref sheet, _row, 1, "Expeses Date");
            report.SetText(ref sheet, _row, 2, headerData["InvoiceDate"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;

            report.SetMasterHeaderText(ref sheet, _row, 1, "Employee");
            report.SetText(ref sheet, _row, 2, headerData["EmployeeName"].ToString());
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            _row++;

            var _rowL = 11;
            if (headerData["BeneficiaryType"].ToString() == BeneficiaryType.Vendor.ToString())
            {
                report.SetMasterHeaderText(ref sheet, _row, 1, "Beneficiary (Vendor)");
                report.SetText(ref sheet, _row, 2, headerData["PartyName"].ToString());
                sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
                _row++;
                _rowL = 12;
            }
            report.SetMasterHeaderText(ref sheet, _row, 1, "Narration");
            report.SetText(ref sheet, _row, 2, headerData["Narration"].ToString());
            sheet.Range[_row, 2].RowHeight = 45;
            sheet[report.GetColumnNameForXls(2) + _row + ":" + report.GetColumnNameForXls(3) + _row].Merge();
            sheet.Range[_row, 1, _row, 3].VerticalAlignment = ExcelVAlign.VAlignTop;

            _row++;
            var _rowR = 5;

            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Voucher Date");
            report.SetText(ref sheet, _rowR, 5, headerData["VoucherDate"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Approved By");
            report.SetText(ref sheet, _rowR, 5, headerData["ApprovedByName"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Checked By");
            report.SetText(ref sheet, _rowR, 5, headerData["ResponsiblePersonName"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();

            _rowR++;
            report.SetMasterHeaderText(ref sheet, _rowR, 4, "Status");
            report.SetText(ref sheet, _rowR, 5, headerData["ApprovalStatus"].ToString());
            sheet[report.GetColumnNameForXls(5) + _rowR + ":" + report.GetColumnNameForXls(6) + _rowR].Merge();
            sheet.Range[_row, 4].VerticalAlignment = ExcelVAlign.VAlignTop;

            var headreColIndex = 1;

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "GL", 30, ExcelHAlign.HAlignCenter);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Budget", 30, ExcelHAlign.HAlignCenter);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Activity", 30, ExcelHAlign.HAlignCenter);
            headreColIndex++;
            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Particulars", 30, ExcelHAlign.HAlignCenter);
            headreColIndex++;

            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            double _Total_Amount = 0;
            report.SetHeaderText(ref sheet, _rowL - 1, headreColIndex, companyCurrencyCode, 11, ExcelHAlign.HAlignCenter);

            report.SetHeaderText(ref sheet, _rowL, headreColIndex, "Amount", ExcelHAlign.HAlignCenter);

            shet2EndxlsCol = headreColIndex;

            double vAmount = 0;
            var data = _expenseBookingService.GetExpenseBookingReportData(expenseBookingId);

            var Row_Total_Start = _rowL + 1;
            for (int n = 0; n < data.Count; n++)
            {
                _rowL++;
                var drcrCol = 1;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["GLGeneralInfoCode"] + " - " + data[n]["GLGeneralInfoName"]); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["BudgetName"].ToString()); drcrCol++;
                report.SetText(ref sheet, _rowL, drcrCol, data[n]["ActivityName"].ToString()); drcrCol++;
                if (!string.IsNullOrEmpty(data[n]["InvoiceNo"].ToString()))
                {
                    report.SetText(ref sheet, _rowL, drcrCol, data[n]["AssetItem"] + " - " + data[n]["InvoiceNo"]); drcrCol++;
                }
                else
                {
                    report.SetText(ref sheet, _rowL, drcrCol, data[n]["AssetItem"].ToString()); drcrCol++;
                }
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

            report.SetText(ref sheet, _rowL, 1, "In Word:", true);
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].Text = report.InWord(_Total_Amount, companyCurrencyId);
            sheet.Range[report.GetColumnNameForXls(2) + _rowL + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + _rowL].Merge();
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[report.GetColumnNameForXls(2) + _rowL].CellStyle.Font.Bold = true;

            _rowL = _rowL + 4;

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, "Expense Voucher", companyId, plantId, plantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            return workbook;
        }
        public IWorkbook GetCashExpenseReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType)
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
                    var glName = dsLocal.Rows[i]["Activity"].ToString();
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
        //Employee payment report old
        //public IWorkbook GetEmployeePayment(out string reportFileName, string companyId, string plantName, string voucherId)
        //{
        //    var excelEngine = new ExcelEngine();
        //    var report = new ReportUtility();
        //    var workbook = report.GetWorkbook(ref excelEngine, 1);
        //    workbook.Version = ExcelVersion.Excel2013;
        //    var sheet = workbook.Worksheets[0];
        //    sheet.Name = "Voucher";

        //    var voucherData = GetEmployeePayablePayment(companyId, voucherId);
        //    var dvapprovedBy = new DataView(voucherData)
        //    {
        //        Sort = "ApprovedBy DESC"
        //    };
        //    var dtApproved = dvapprovedBy.ToTable(true, "ApprovedBy");
        //    var dvemployee = new DataView(voucherData)
        //    {
        //        Sort = "Employee DESC"
        //    };
        //    var dtemployee = dvemployee.ToTable(true, "Employee");
        //    var transactionCurrencyId = voucherData.Rows[0]["CurrencyId"].ToString();

        //    _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

        //    var _col = 1;
        //    var row = 5;
        //    var shet2EndxlsCol = _col;

        //    // Set report Name
        //    reportFileName = Convert.ToDateTime(voucherData.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + voucherData.Rows[0]["VoucherNo"];

        //    report.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
        //    report.SetText(ref sheet, row, 2, voucherData.Rows[0]["VoucherNo"].ToString());
        //    sheet[report.GetColumnNameForXls(2) + row + ":" + report.GetColumnNameForXls(2 + 1) + row].Merge();

        //    report.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
        //    report.SetText(ref sheet, row, 5, voucherData.Rows[0]["VoucherDate"].ToString());
        //    sheet[report.GetColumnNameForXls(5) + row + ":" + report.GetColumnNameForXls(5 + 1) + row].Merge();
        //    row++;

        //    report.SetMasterHeaderText(ref sheet, row, 1, "Doc Date");
        //    report.SetText(ref sheet, row, 2, voucherData.Rows[0]["DocDate"].ToString());
        //    sheet[report.GetColumnNameForXls(2) + row + ":" + report.GetColumnNameForXls(2 + 1) + row].Merge();

        //    report.SetMasterHeaderText(ref sheet, row, 4, "Doc No");
        //    report.SetText(ref sheet, row, 5, voucherData.Rows[0]["DocRefNo"].ToString());
        //    sheet[report.GetColumnNameForXls(5) + row + ":" + report.GetColumnNameForXls(5 + 1) + row].Merge();
        //    row++;

        //    report.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
        //    report.SetText(ref sheet, row, 2, voucherData.Rows[0]["PostingDate"].ToString());
        //    sheet[report.GetColumnNameForXls(2) + row + ":" + report.GetColumnNameForXls(2 + 1) + row].Merge();

        //    report.SetMasterHeaderText(ref sheet, row, 4, "Fiscal Year");
        //    report.SetText(ref sheet, row, 5, voucherData.Rows[0]["PeriodName"] + " (" + voucherData.Rows[0]["PeriodNo"] + ")");
        //    sheet[report.GetColumnNameForXls(5) + row + ":" + report.GetColumnNameForXls(5 + 1) + row].Merge();
        //    row++;

        //    report.SetMasterHeaderText(ref sheet, row, 1, "Employee");
        //    report.SetText(ref sheet, row, 2, dtemployee.Rows[0]["Employee"].ToString());
        //    sheet[report.GetColumnNameForXls(2) + row + ":" + report.GetColumnNameForXls(2 + 1) + row].Merge();

        //    //report.SetMasterHeaderText(ref sheet, row, 4, "Approved By");
        //    //report.SetText(ref sheet, row, 5, dtApproved.Rows[0]["ApprovedBy"].ToString());
        //    //sheet[report.GetColumnNameForXls(5) + row + ":" + report.GetColumnNameForXls(5 + 1) + row].Merge();
        //    //row++;

        //    report.SetMasterHeaderText(ref sheet, row, 1, "Narration");
        //    report.SetText(ref sheet, row, 2, voucherData.Rows[0]["Narration"].ToString());
        //    sheet[report.GetColumnNameForXls(2) + row + ":" + report.GetColumnNameForXls(2 + 1) + (row + 1)].Merge();

        //    report.SetMasterHeaderText(ref sheet, row, 4, "Status");
        //    report.SetText(ref sheet, row, 5, Convert.ToBoolean(voucherData.Rows[0]["IsPark"].ToString()) ? "Parked" : "Posted");
        //    sheet[report.GetColumnNameForXls(5) + row + ":" + report.GetColumnNameForXls(5 + 1) + row].Merge();
        //    row += 2;

        //    report.SetHeaderText(ref sheet, row, 1, "GL", 15);
        //    report.SetHeaderText(ref sheet, row, 2, "Budget", 14);
        //    report.SetHeaderText(ref sheet, row, 3, "Activity", 14);
        //    report.SetHeaderText(ref sheet, row, 4, "Detail Narration", 14);
        //    if (companyCurrencyId != transactionCurrencyId)
        //    {
        //        report.SetHeaderText(ref sheet, row, 5, "Trn Currency", 10, ExcelHAlign.HAlignLeft);
        //        report.SetHeaderText(ref sheet, row, 6, "Trn Value", 9, ExcelHAlign.HAlignRight);
        //    }
        //    report.SetHeaderText(ref sheet, row - 1, 5, companyCurrencyCode, 10, ExcelHAlign.HAlignCenter);
        //    sheet[row - 1, 5, row - 1, 6].Merge();

        //    report.SetHeaderText(ref sheet, row, 5, "Debit", ExcelHAlign.HAlignRight);
        //    report.SetHeaderText(ref sheet, row, 6, "Credit", ExcelHAlign.HAlignRight);

        //    shet2EndxlsCol = 6;

        //    double totalAmount = 0;
        //    row++;
        //    var Row_Total_Start = row;
        //    for (int n = 0; n < voucherData.Rows.Count; n++)
        //    {
        //        if (!string.IsNullOrEmpty(voucherData.Rows[n]["AccountTitle"].ToString()))
        //        {
        //            report.SetText(ref sheet, row, 1, voucherData.Rows[n]["GLGeneralInfoCode"] + " - " + voucherData.Rows[n]["AccountTitle"]);
        //        }
        //        else
        //        {
        //            report.SetText(ref sheet, row, 1, voucherData.Rows[n]["GLGeneralInfoCode"] + " - " + voucherData.Rows[n]["GL"]);
        //        }
        //        report.SetText(ref sheet, row, 2, voucherData.Rows[n]["Budget"].ToString());
        //        report.SetText(ref sheet, row, 3, voucherData.Rows[n]["Activity"].ToString());
        //        report.SetText(ref sheet, row, 4, voucherData.Rows[n]["DetailNarration"].ToString());
        //        sheet[row, 4].ColumnWidth = 55;



        //        if (companyCurrencyId != transactionCurrencyId)
        //        {
        //            report.SetText(ref sheet, row, 5, voucherData.Rows[n]["TrnCurrency"].ToString());
        //            report.SetText(ref sheet, row, 6, Convert.ToDouble(voucherData.Rows[n]["DrAmount"]));
        //            totalAmount += Convert.ToDouble(voucherData.Rows[n]["DrAmount"].ToString());
        //        }
        //        else
        //        {
        //            report.SetText(ref sheet, row, 5, Convert.ToDouble(voucherData.Rows[n]["CompanyCurrencyDrAmount"].ToString()));
        //            report.SetText(ref sheet, row, 6, Convert.ToDouble(voucherData.Rows[n]["CompanyCurrencyCrAmount"].ToString()));
        //            totalAmount += Convert.ToDouble(voucherData.Rows[n]["CompanyCurrencyDrAmount"].ToString());
        //        }
        //        sheet[row, 4].RowHeight = 45;
        //        //cellA2.WrapText = true;
        //        sheet[row,4].WrapText = true;

        //        row++;
        //    }




        //    var rowLast = row - 1;
        //    var colLast = 6;
        //    sheet.Range[report.GetColumnNameForXls(1) + row + ": " + report.GetColumnNameForXls(colLast - 2) + row].Merge();
        //    report.SetText(ref sheet, row, 1, "Total : ", true);

        //    sheet.Range[row, colLast - 1].Formula = "=SUM(" + report.GetColumnNameForXls(colLast - 1) + Row_Total_Start + ":" + report.GetColumnNameForXls(colLast - 1) + rowLast + ")";
        //    sheet.Range[row, colLast - 1].NumberFormat = report.NumberFormatDecimalTwo();
        //    sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
        //    sheet.Range[row, colLast - 1].BorderAround(ExcelLineStyle.Hair);

        //    sheet.Range[row, colLast].Formula = "=SUM(" + report.GetColumnNameForXls(colLast) + Row_Total_Start + ":" + report.GetColumnNameForXls(colLast) + rowLast + ")";
        //    sheet.Range[row, colLast].NumberFormat = report.NumberFormatDecimalTwo();
        //    sheet.Range[row, colLast].CellStyle.Font.Bold = true;
        //    sheet.Range[row, colLast].BorderAround(ExcelLineStyle.Hair);

        //    sheet.Range[11, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
        //    sheet.Range[11, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
        //    row += 2;

        //    report.SetText(ref sheet, row, 1, "In Word:", true);

        //    sheet.Range[report.GetColumnNameForXls(2) + row].Text = report.InWord(totalAmount, transactionCurrencyId);
        //    sheet.Range[report.GetColumnNameForXls(2) + row + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + row].Merge();
        //    sheet.Range[report.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //    sheet.Range[report.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
        //    sheet.Range[report.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

        //    if (companyCurrencyId != transactionCurrencyId)
        //    {
        //        row++;
        //        sheet.Range[report.GetColumnNameForXls(2) + row].Text = report.InWord(totalAmount, transactionCurrencyId);
        //        sheet.Range[report.GetColumnNameForXls(2) + row + ":" + report.GetColumnNameForXls(shet2EndxlsCol) + row].Merge();
        //        sheet.Range[report.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //        sheet.Range[report.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
        //        sheet.Range[report.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
        //    }

        //    row += 3;
        //    report.SetSignatureText(ref sheet, row - 1, 1, voucherData.Rows[0]["AddedBy"].ToString());
        //    sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //    report.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

        //    report.SetSignatureText(ref sheet, row - 1, 3, voucherData.Rows[0]["PostedBy"].ToString());
        //    sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //    report.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

        //    sheet.Range[row, shet2EndxlsCol].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        //    report.SetTextMiddle(ref sheet, row, shet2EndxlsCol, "HOD (Finance)", true);

        //    sheet.UsedRange.AutofitColumns();
        //    sheet.UsedRange.CellStyle.Font.Size = 8;
        //    report.CompanyPlantHeader(ref sheet, shet2EndxlsCol, "Employee Payment", companyId, plantName, null);
        //    report.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);

        //    sheet[row, 4].ColumnWidth = 55;
        //    return workbook;
        //}

        #region Employee payment report 
        //Employee payment report new format header data
        private Dictionary<string, object> GetEmployeePaymentHeader(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo
                            ,EI.EmployeeCode,EI.EmployeeName
                            ,AddedBy=CASE WHEN U.FullName<>'' THEN U.FullName ELSE V.AddedBy END
                            ,PostedBy=CASE WHEN U1.FullName<>'' THEN U1.FullName ELSE V.PostedBy END
                            , UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , EPW.CurrencyId, C.Code AS CurrencyCode
                            FROM [TRN].[EmployeePayableWriteOff] AS EPW
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=EPW.VoucherId
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=EPW.EmployeeId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            LEFT JOIN SEC.[User] U ON U.UserId=V.AddedBy
                            LEFT JOIN SEC.[User] U1 ON U1.UserId=V.PostedBy
                          WHERE EPW.Archive=0 AND EPW.CompanyGroupId='" + companyGroupId + "' AND EPW.CompanyId='" + companyId + "' AND EPW.PlantId='" + plantId + "' AND EPW.VoucherId='" + voucherId + "'  AND EPW.SourceType='EmployeePayment'";
            return _sqlRepository.GetData(cmdText);
        }


        //Employee Payment report new GL data old and New
        private DataTable GetEmployeePaymentVoucher(string companyId, string voucherId)
        {
            try
            {
                var sql = @"SELECT DISTINCT VD.EmployeePayableWriteOffDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate
                        , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, V.VoucherNo, V.Narration, VD.Narration AS DetailNarration, V.CurrencyId, CU.Code AS TrnCurrency, V.AddedBy, V.PostedBy
                        , GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GL, BM.AccountTitle, BM.AccountNumber, ENT.UserName AS Entity, BUD.UserName AS Budget

                            ,Activity=CASE WHEN VD.CashMasterId<>'' THEN  CM.UserName  WHEN VD.BankMasterId<>'' THEN BNM.AccountTitle Else ACT.UserName end 
                            ,CM.UserName AS CashMasterName


                        --, ACT.UserName AS Activity
                        , EMP.EmployeeName AS Employee
                        ,ApprovedBy=CASE WHEN CI.InventoryReceiveId<>'' THEN EMPGRN.EmployeeName ELSE   EAHX.EmployeeName END
						, VD.DrAmount, VD.CrAmount, ISNULL(CC.CompanyCurrencyDrAmount,0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount,0) AS CompanyCurrencyCrAmount
                        
						FROM [TRN].[VoucherDetail] AS VD
                        JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [TRN].[EmployeePayableWriteOffDetail] AS IWD ON IWD.Id=VD.EmployeePayableWriteOffDetailId
                        LEFT JOIN [TRN].[EmployeePayableWriteOff] AS IW ON IW.Id=IWD.EmployeePayableWriteOffId
                        LEFT JOIN [TRN].[EmployeePayable] AS CI ON CI.Id=IWD.EmployeePayableId
                        LEFT JOIN [TRN].[ExpenseBooking] As EB ON EB.Id=CI.ExpenseBookingId
                        LEFT JOIN (SELECT EBA.EmployeeName , EAH.ExpenseBookingId, EAH.ExpenseBookingDetailId  FROM [TRN].[ExpenseBookingApprovalHistory] As EAH
	                        LEFT JOIN [dbo].[EmployeeInformation] AS EBA ON EBA.SystemId=EAH.EmployeeId
                        )AS EAHX ON EAHX.ExpenseBookingId=EB.Id
                        LEFT JOIN [dbo].[EmployeeInformation] AS EMP ON EMP.SystemId=VD.EmployeeId
						LEFT JOIN TRN.InventoryReceive GRN ON GRN.Id=CI.InventoryReceiveId
                        LEFT JOIN [dbo].[EmployeeInformation] AS EMPGRN ON EMPGRN.SystemId=GRN.AuthorizedBy
                        LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=V.CurrencyId
                        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [HKP].[Bank] BN ON BN.Id=BM.BankId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUDM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                        LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id=VD.EntityId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                            LEFT JOIN [MST].[CashMaster] AS CM ON CM.Id=VD.CashMasterId
                            LEFT JOIN [MST].[BankMaster] AS BNM ON BNM.Id=VD.BankMasterId

                        WHERE V.Archive=0 AND V.SourceType='" + SourceType.EmployeePayment + "' AND V.Id='" + voucherId + "' AND V.CompanyId='" + companyId + "' ORDER BY VD.DrAmount DESC";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Employee Payment report  New
        public IWorkbook GetEmployeePayment(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetEmployeePaymentHeader(companyGroupId, companyId, plantId, voucherId, SourceType.EmployeePayment);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            var dsLocal = GetEmployeePaymentVoucher(companyId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;
            var colLast = 1;
            int xlsCol = 1;

            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            sheet[row, 1].ColumnWidth = 20;
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString());
            sheet[row, 2].ColumnWidth = 10;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            sheet[row, 3].ColumnWidth = 10;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 7, header["VoucherDate"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
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

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Employee:");
            reportUtility.SetText(ref sheet, row, 2, header["EmployeeCode"].ToString() + " - " + header["EmployeeName"].ToString());
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 7, header["DocRefNo"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;
            row++;

            //reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Customer Plant");
            // reportUtility.SetText(ref sheet, row, 2, header["CustomerPlant"].ToString());

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            sheet.Range[row, 1].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

            reportUtility.SetMasterHeaderText(ref sheet, row, 6, "Status");
            reportUtility.SetText(ref sheet, row, 7, header["Status"].ToString());
            sheet.Range[row, 6].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, 7].VerticalAlignment = ExcelVAlign.VAlignTop;

            //row++;
            row++;  //10
            colLast = companyCurrencyId == transcationCurrency ? 7 : 9;
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
            //sheet[row, 6].RowHeight = 15;

            sheet.Range[row, 6, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[row, 6, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row++;

            int colGl = 0;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++; //clo3

            xlsCol++; //cloDNaration
            int colDnaration = 0;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Detail Narration"); colDnaration = xlsCol;
            sheet[row, 4].ColumnWidth = 40;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            xlsCol++; //clo5
            int colApprovedBy = 0;
            colApprovedBy = xlsCol;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Approved By");
            sheet[row, colApprovedBy].ColumnWidth = 20;
            //sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();
            xlsCol++;

            //xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol; //col9

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, colGl, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }
            else
            {

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;

                //sheet.Range[row, 4, row, colLast].BorderAround(ExcelLineStyle.Thin);
                //sheet.Range[row, 4, row, colLast].BorderInside(ExcelLineStyle.Thin);

                sheet.Range[row, colGl, row, colLast].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[row, colGl, row, colLast].BorderInside(ExcelLineStyle.Hair);
                //sheet.Range[row, 4, row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            }


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
                    // glName = string.Empty;
                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);
                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(colGl + 2) + row].Merge();

                    reportUtility.SetText(ref sheet, row, colDnaration, dsLocal.Rows[i]["DetailNarration"].ToString());
                    sheet[row, colDnaration].RowHeight = 25;
                    sheet[row, colDnaration].WrapText = true;

                    reportUtility.SetText(ref sheet, row, colApprovedBy, dsLocal.Rows[i]["ApprovedBy"].ToString());

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

                    // glName = string.Empty;

                    // sheet.AutofitRow(3);



                    row++;
                }

                formulaEndRow = row - 1;
                reportUtility.SetText(ref sheet, row, 5, "Total: ", true);

                if (companyCurrencyId != transcationCurrency)
                {
                    //worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                    //worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    //worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    //worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

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

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + formulaStartRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (formulaEndRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
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
                }

                sheet.Range[row, colinrDebit, row, colLast].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[row, colinrDebit, row, colLast].BorderAround(ExcelLineStyle.Hair);

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                    sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;
                    row++;

                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                // sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                sheet.Range[row, 2].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sheet.UsedRange.AutofitColumns();

                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;

                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);
                sheet[row, 1].ColumnWidth = 21;

                // reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["AddedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Received By", true);
                //sheet[row, 3].ColumnWidth = 15;



                reportUtility.SetSignatureText(ref sheet, row - 1, 5, header["PostedBy"].ToString());
                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Checked By", true);
                //sheet[row, 5].ColumnWidth = 15;

                sheet.Range[row, 7].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 7, "Authorized By", true);
                sheet[row, 6].ColumnWidth = 15;
                sheet[row, 7].ColumnWidth = 15;

                sheet[row, 8].ColumnWidth = 15;
                sheet[row, 9].ColumnWidth = 15;


                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
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
                reportUtility.CompanyPlantHeader(ref sheet, 9, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 9, ExcelPageOrientation.Portrait);
            }

            return workbook;
        }
        #endregion Employee payment report 




        public DataTable CompanyHeader(string companyId)
        {
            var sql = @"SELECT COM.Id, COM.UserName, COM.LegalName, COM.WebDomain, AM.Address1, AM.Address2, CO.UserName AS Country, CT.UserName AS City, CM.Phone1 AS Phone, CM.Email1 AS Email
                        , CM.Website AS Website, AR.UserName AS Area
                        , [Address]=CASE ISNULL(AM.Address1,'') WHEN '' THEN '' ELSE AM.Address1 +', ' END+
			                        CASE ISNULL(AR.UserName,'') WHEN '' THEN '' ELSE AR.UserName +', ' END+
			                        CASE ISNULL(CT.UserName,'') WHEN '' THEN '' ELSE ct.UserName END
                        , Contact=CASE ISNULL(CM.Phone1,'') WHEN '' THEN '' ELSE CM.Phone1 +', ' END+
		                        CASE ISNULL(CM.Email1,'') WHEN '' THEN '' ELSE CM.Email1 +', ' END+
		                        CASE ISNULL(CM.Website ,'') WHEN '' THEN '' ELSE CM.Website  END
                        FROM [ORG].[Company] AS COM
                        LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=COM.AddressMasterId
                        LEFT JOIN [MST].[ContactMaster] AS CM ON CM.Id=COM.ContactMasterId
                        LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
                        LEFT JOIN [SCS].[City] AS CT ON CT.Id=AM.CityId
                        LEFT JOIN [SCS].[Area] AS AR ON AR.Id=AM.AreaId
                        WHERE COM.Id='" + companyId + "'";
            return _sqlRepository.GetDataTable(sql);
        }
        //Employee payment report data old and new format
        public DataTable GetEmployeePayablePayment(string companyId, string voucherId)
        {
            var sql = @"SELECT DISTINCT VD.EmployeePayableWriteOffDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate
                        , V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, V.VoucherNo, V.Narration, VD.Narration AS DetailNarration, V.CurrencyId, CU.Code AS TrnCurrency, V.AddedBy, V.PostedBy
                        , GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GL, BM.AccountTitle, BM.AccountNumber, ENT.UserName AS Entity, BUD.UserName AS Budget, ACT.UserName AS Activity, EMP.EmployeeName AS Employee
                        ,ApprovedBy=CASE WHEN CI.InventoryReceiveId<>'' THEN EMPGRN.EmployeeName ELSE   EAHX.EmployeeName END
						, VD.DrAmount, VD.CrAmount, ISNULL(CC.CompanyCurrencyDrAmount,0) AS CompanyCurrencyDrAmount, ISNULL(CC.CompanyCurrencyCrAmount,0) AS CompanyCurrencyCrAmount
                        
						FROM [TRN].[VoucherDetail] AS VD
                        JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                        LEFT JOIN [TRN].[EmployeePayableWriteOffDetail] AS IWD ON IWD.Id=VD.EmployeePayableWriteOffDetailId
                        LEFT JOIN [TRN].[EmployeePayableWriteOff] AS IW ON IW.Id=IWD.EmployeePayableWriteOffId
                        LEFT JOIN [TRN].[EmployeePayable] AS CI ON CI.Id=IWD.EmployeePayableId
                        LEFT JOIN [TRN].[ExpenseBooking] As EB ON EB.Id=CI.ExpenseBookingId
                        LEFT JOIN (SELECT EBA.EmployeeName , EAH.ExpenseBookingId, EAH.ExpenseBookingDetailId  FROM [TRN].[ExpenseBookingApprovalHistory] As EAH
	                        LEFT JOIN [dbo].[EmployeeInformation] AS EBA ON EBA.SystemId=EAH.EmployeeId
                        )AS EAHX ON EAHX.ExpenseBookingId=EB.Id
                        LEFT JOIN [dbo].[EmployeeInformation] AS EMP ON EMP.SystemId=VD.EmployeeId
						LEFT JOIN TRN.InventoryReceive GRN ON GRN.Id=CI.InventoryReceiveId
                        LEFT JOIN [dbo].[EmployeeInformation] AS EMPGRN ON EMPGRN.SystemId=GRN.AuthorizedBy
                        LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=V.CurrencyId
                        LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                        LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                        LEFT JOIN [MST].[BankMaster] AS BM ON BM.Id=VD.BankMasterId
                        LEFT JOIN [HKP].[Bank] BN ON BN.Id=BM.BankId
                        LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                        LEFT JOIN [MST].[BudgetMaster] AS BUDM ON BUDM.Id = VD.BudgetMasterId
                        LEFT JOIN [HKP].[Budget] AS BUD ON BUD.Id=BUDM.BudgetId
                        LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id=VD.ActivityId
                        LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id=VD.EntityId
                        LEFT JOIN (SELECT VDC.VoucherDetailId, VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount
	                        FROM [TRN].[VoucherDetailCurrency] AS VDC
	                        JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
	                        WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
                        ) AS CC ON CC.VoucherDetailId=VD.Id
                        WHERE V.Archive=0 AND V.SourceType='" + SourceType.EmployeePayment + "' AND V.Id='" + voucherId + "' AND V.CompanyId='" + companyId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(sql);
        }

    }
}