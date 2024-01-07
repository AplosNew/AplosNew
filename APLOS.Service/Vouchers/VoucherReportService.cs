using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Service.Accounts;
using Library.Service.ChartOfAccounts;
using Library.Service.Currencies;
using Library.Service.Helpers;
using Library.Service.ManagementChartOfAccounts;
using Library.Service.Organizations;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.Service.Vouchers
{
    public class VoucherReportService : IVoucherReportService
    {
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IGLGeneralInfoService _gLGeneralInfoService;
        private readonly IBudgetMasterService _budgetMasterService;
        private readonly IVoucherService _voucherService;
        private readonly IPlantService _plantService;
        private readonly IActivityService _activityService;
        //private readonly AccountVoucherReportService _accountVoucherReportService;

        public VoucherReportService(ISqlRepository sqlRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IGLGeneralInfoService gLGeneralInfoService
            , IBudgetMasterService budgetMasterService
            , IVoucherService voucherService
            , IPlantService plantService
            , IActivityService activityService
            //, AccountVoucherReportService accountVoucherReportService
            )
        {
            _sqlRepository = sqlRepository;
            _voucherService = voucherService;
            _gLGeneralInfoService = gLGeneralInfoService;
            _budgetMasterService = budgetMasterService;
            _activityService = activityService;
            _plantService = plantService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
           // _accountVoucherReportService = accountVoucherReportService;
        }

       
        
       
       
        public IWorkbook GetGLVoucher(out ExcelEngine excelEngine, string masterId)
        {
            try
            {
                var obj = new ReportGeneralVoucher();
                excelEngine = new ExcelEngine();
                var workbook = obj.GL_Voucher(ref excelEngine, masterId);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetCoa(out ExcelEngine excelEngine, string masterId)
        {
            try
            {
                var obj = new ReportGeneralVoucher();
                excelEngine = new ExcelEngine();
                var workbook = obj.Coa_Report(ref excelEngine);
                return workbook;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IWorkbook GetGLDateWise(out ExcelEngine excelEngine, string masterId, string fromDate, string toDate)
        {
            var obj = new ReportGeneralVoucher();
            excelEngine = new ExcelEngine();
            var workbook = obj.GL_DateRangeWise(ref excelEngine, masterId, fromDate, toDate);
            return workbook;
        }


        public IWorkbook GetIncomeStatementReport(string companyId, string plantId, string plantName, string date, string[] parallelCurrencies, bool isBudgetLevel, bool isActivityLevel)
        {
            try
            {
                var obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    var workbook = obj.IncomeStatement_Report(excelEngine, companyId,  plantId,plantName, date, parallelCurrencies,isBudgetLevel, isActivityLevel);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IWorkbook GetIncomeStatementYearClosedReport(string companyId, string plantId, string plantName, string fiscalYearCloseId, string fiscalYearName, bool isBudgetLevel, bool isActivityLevel)
        {
            try
            {
                var obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    var workbook = obj.IncomeStatement_YearClosed_Report(excelEngine, companyId, plantId, plantName, fiscalYearCloseId, fiscalYearName, isBudgetLevel, isActivityLevel);
                    return workbook;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Income statement datewise
        public IWorkbook GetIncomeStatementReportDateWise(string companyId, string PlantId, string plantName, string fromDate, string toDate,  string[] parallelCurrencies, bool isBudgetLevel, bool isActivityLevel)
        {
            try
            {
                var obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    var workbook = obj.IncomeStatement_Report_DateRange(excelEngine, companyId, PlantId, plantName, fromDate, toDate,  parallelCurrencies, isBudgetLevel, isActivityLevel);
                    return workbook;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetEntityWiseExpenseAndEarningReportDateWise(string companyId, string PlantId, string plantName, string fromDate, string toDate,  string entityId, string entity, string[] parallelCurrencies)
        {
            try
            {
                var obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    var workbook = obj.EntityWiseExpenseandEarning_Report_DateRange(excelEngine, companyId, PlantId, plantName, fromDate, toDate, entityId, entity,parallelCurrencies);
                    return workbook;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IWorkbook EntityWiseExpenseAndEarningreportDateWiseActivityLevel(string companyId, string PlantId, string plantName, string fromDate, string toDate, string entityId, string entity, string[] parallelCurrencies)
        {
            try
            {
                var obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    var workbook = obj.EntityWiseExpenseandEarning_Report_DateRange_ActivityLevel(excelEngine, companyId, PlantId, plantName, fromDate, toDate, entityId, entity, parallelCurrencies);
                    return workbook;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region balance sheet Date Range
        public IWorkbook GetBalanceSheetReportDateWise(string companyId, string plantName, string fromDate, string toDate)
        {
            try
            {
                var obj = new ReportGeneralVoucher();
                using (ExcelEngine excelEngine = new ExcelEngine())
                {
                    var workbook = obj.BalanceSheet_Report_DateRange(excelEngine, companyId, plantName, fromDate, toDate);
                    return workbook;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #endregion balance sheet

        public IWorkbook GetGeneralVoucher(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var dsLocal = _voucherService.GetJournalData(companyGroupId, companyId, plantId, voucherId);
            if (dsLocal.Rows.Count == 0)
                throw new Exception("No data found !");

            // Set report Name
            reportFileName = Convert.ToDateTime(dsLocal.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dsLocal.Rows[0]["VoucherNo"];

            var curCode = dsLocal.Rows[0]["CurrencyCode"].ToString();
            var trnCur = dsLocal.Rows[0]["TrnCurrency"].ToString();
            var row = 5;

            //var colLast = 1;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, dsLocal.Rows[0]["VoucherNo"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

          
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, dsLocal.Rows[0]["VoucherDate"].ToString());
            if (curCode != trnCur)
            {
                sheet[reportUtility.GetColumnNameForXls(5) + 8 + ":" + reportUtility.GetColumnNameForXls(6) + 8].Merge();
            }

            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Doc Date");
            reportUtility.SetText(ref sheet, row, 2, dsLocal.Rows[0]["DocDate"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc No");
            reportUtility.SetText(ref sheet, row, 5, dsLocal.Rows[0]["DocRefNo"].ToString());
            if (curCode != trnCur)
            {
                sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            }

            row++;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, dsLocal.Rows[0]["PostingDate"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Fiscal Year");
            reportUtility.SetText(ref sheet, row, 5, dsLocal.Rows[0]["FiscalYearName"] + " (" + dsLocal.Rows[0]["PeriodNo"] + ")");
            if (curCode != trnCur)
            {
                sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();
            }

            row++; //row8
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, dsLocal.Rows[0]["Narration"].ToString());
            //sheet[reportUtility.GetColumnNameForXls(4) + row + ":" + reportUtility.GetColumnNameForXls(5) + row].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Status");
            reportUtility.SetText(ref sheet, row, 5, dsLocal.Rows[0]["Park/Post"].ToString());

            row++; //row9

            
       

            sheet.Range[9, 4, row, 5].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[9, 4, row, 5].BorderInside(ExcelLineStyle.Hair);
            row++;
            var col = 1;
            reportUtility.SetHeaderText(ref sheet, 10, col, "GL", 15); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Budget", 15); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Activity", 15); col++;
            sheet[10, 1, 10,3].Merge();
            if (curCode != trnCur)
            {
                reportUtility.SetHeaderText(ref sheet, 10, col, "Trn Currency", 7); col++;
                reportUtility.SetHeaderText(ref sheet, 10, col, "Trn Value", 9); col++;
            }
            reportUtility.SetHeaderText(ref sheet, 9, col, curCode, ExcelHAlign.HAlignCenter);
            sheet[9, col, 9, col + 1].Merge();
            reportUtility.SetHeaderText(ref sheet, 10, col, "Debit", 11, ExcelHAlign.HAlignRight); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Credit", 11, ExcelHAlign.HAlignRight);
            var colLast = col;
            //row = 11;
            row++;

            double _Total_Amount = 0;
            var Row_Total_Start = row; //row11
            for (int n = 0; n < dsLocal.Rows.Count; n++)
            {
                col = 1;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["AccountCode"] + " - " + dsLocal.Rows[n]["GL"] + " - " + dsLocal.Rows[n]["BudgetName"] + " - " + dsLocal.Rows[n]["Activity"]); col++; //GL
              
                col++;
                col++;
                if (curCode != trnCur)
                {
                    reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["TrnCurrency"].ToString()); col++;
                    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["Value"])); col++;
                }
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["DrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(dsLocal.Rows[n]["DrAmount"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["CrAmount"].ToString())); col++;
                sheet[row, 1, row, 3].Merge();
                row++;
            }

            var rowLast = row - 1;
            sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(colLast - 2) + row].Merge();
            reportUtility.SetText(ref sheet, row, 1, "Total : ", true);

            sheet.Range[row, colLast - 1].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colLast - 1) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(colLast - 1) + rowLast + ")"; //rowLast
            sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
            sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
           // sheet.Range[row, colLast - 1].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[row, colLast].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colLast) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(colLast) + rowLast + ")";
            sheet.Range[row, colLast].NumberFormat = reportUtility.NumberFormatDecimalTwo();
            sheet.Range[row, colLast].CellStyle.Font.Bold = true;
            //sheet.Range[row, colLast].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[10, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[10, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row += 2;

            reportUtility.SetText(ref sheet, row, 1, "In Word : ", true);
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(_Total_Amount, dsLocal.Rows[0]["CurrencyId"].ToString()); ;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            row = row + 4;

            reportUtility.SetSignatureText(ref sheet, row - 1, 1, dsLocal.Rows[0]["AddedBy"].ToString());
            sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

            reportUtility.SetSignatureText(ref sheet, row - 1, 3, dsLocal.Rows[0]["PostedBy"].ToString());
            sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

            sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

            reportUtility.CompanyPlantHeader(ref sheet, colLast, "Journal Voucher", companyId, plantId, plantName, null);
            reportUtility.FreezePage(ref sheet, 1, colLast);
            reportUtility.PageAdjustableSetup(ref sheet, 1, row + 3, ExcelPageOrientation.Portrait);



            sheet[1, 2].ColumnWidth = 30;
            sheet[1, 3].ColumnWidth = 20;
            return workbook;



        }
       
        
       

        public IWorkbook GetOBAdvanceJournalVoucher(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string openingBalanceId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var dsLocal = _voucherService.GetOBAdvanceJournalData(companyGroupId, companyId, plantId, openingBalanceId);
            if (dsLocal.Rows.Count == 0)
                throw new Exception("No data found !");

            // Set report Name
            reportFileName = Convert.ToDateTime(dsLocal.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + dsLocal.Rows[0]["DocRefNo"];

            var curCode = dsLocal.Rows[0]["CurrencyCode"].ToString();
            var trnCur = dsLocal.Rows[0]["TrnCurrency"].ToString();

            reportUtility.SetMasterHeaderText(ref sheet, 5, 1, "Voucher No");
            reportUtility.SetText(ref sheet, 5, 2, dsLocal.Rows[0]["VoucherNo"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + 5 + ":" + reportUtility.GetColumnNameForXls(3) + 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 5, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, 5, 5, dsLocal.Rows[0]["VoucherDate"].ToString());
            if (curCode != trnCur)
            {
                sheet[reportUtility.GetColumnNameForXls(5) + 5 + ":" + reportUtility.GetColumnNameForXls(6) + 5].Merge();
            }

            reportUtility.SetMasterHeaderText(ref sheet, 6, 1, "Doc Date");
            reportUtility.SetText(ref sheet, 6, 2, dsLocal.Rows[0]["DocDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + 6 + ":" + reportUtility.GetColumnNameForXls(3) + 6].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 6, 4, "Doc No");
            reportUtility.SetText(ref sheet, 6, 5, dsLocal.Rows[0]["DocRefNo"].ToString());
            if (curCode != trnCur)
            {
                sheet[reportUtility.GetColumnNameForXls(5) + 6 + ":" + reportUtility.GetColumnNameForXls(6) + 6].Merge();
            }

            reportUtility.SetMasterHeaderText(ref sheet, 7, 1, "Posting Date");
            reportUtility.SetText(ref sheet, 7, 2, dsLocal.Rows[0]["PostingDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + 7 + ":" + reportUtility.GetColumnNameForXls(3) + 7].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 7, 4, "Fiscal Year");
            reportUtility.SetText(ref sheet, 7, 5, dsLocal.Rows[0]["FiscalYearName"] + " (" + dsLocal.Rows[0]["PeriodNo"] + ")");
            if (curCode != trnCur)
            {
                sheet[reportUtility.GetColumnNameForXls(5) + 7 + ":" + reportUtility.GetColumnNameForXls(6) + 7].Merge();
            }

            reportUtility.SetMasterHeaderText(ref sheet, 8, 1, "Narration");
            reportUtility.SetText(ref sheet, 8, 2, dsLocal.Rows[0]["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + 8 + ":" + reportUtility.GetColumnNameForXls(3) + 9].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 8, 4, "Status");
            reportUtility.SetText(ref sheet, 8, 5, dsLocal.Rows[0]["Park/Post"].ToString());
            if (curCode != trnCur)
            {
                sheet[reportUtility.GetColumnNameForXls(5) + 8 + ":" + reportUtility.GetColumnNameForXls(6) + 8].Merge();
            }

            var col = 1;
            reportUtility.SetHeaderText(ref sheet, 10, col, "GL", 28); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Budget", 15); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Activity", 15); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Particular", 15); col++;
            if (curCode != trnCur)
            {
                reportUtility.SetHeaderText(ref sheet, 10, col, "Trn Currency", 7); col++;
                reportUtility.SetHeaderText(ref sheet, 10, col, "Trn Value", 9); col++;
            }
            reportUtility.SetHeaderText(ref sheet, 9, col, curCode, ExcelHAlign.HAlignCenter);
            sheet[9, col, 9, col + 1].Merge();
            reportUtility.SetHeaderText(ref sheet, 10, col, "Debit", 11, ExcelHAlign.HAlignRight); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Credit", 11, ExcelHAlign.HAlignRight);
            var colLast = col;
            var row = 11;

            double _Total_Amount = 0;
            var Row_Total_Start = row;
            for (int n = 0; n < dsLocal.Rows.Count; n++)
            {
                col = 1;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["AccountCode"] + " - " + dsLocal.Rows[n]["GL"]); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["BudgetName"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["Activity"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["ParticularName"].ToString()); col++;
                if (curCode != trnCur)
                {
                    reportUtility.SetText(ref sheet, row, col, dsLocal.Rows[n]["TrnCurrency"].ToString()); col++;
                    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["Value"])); col++;
                }
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["DrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(dsLocal.Rows[n]["DrAmount"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(dsLocal.Rows[n]["CrAmount"].ToString())); col++;
                row++;
            }

            var rowLast = row - 1;
            sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(colLast - 2) + row].Merge();
            reportUtility.SetText(ref sheet, row, 1, "Total : ", true);

            sheet.Range[row, colLast - 1].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colLast - 1) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(colLast - 1) + rowLast + ")";
            sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
            sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
            sheet.Range[row, colLast - 1].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[row, colLast].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colLast) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(colLast) + rowLast + ")";
            sheet.Range[row, colLast].NumberFormat = reportUtility.NumberFormatDecimalTwo();
            sheet.Range[row, colLast].CellStyle.Font.Bold = true;
            sheet.Range[row, colLast].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[11, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[11, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row += 2;

            reportUtility.SetText(ref sheet, row, 1, "In Word : ", true);
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(_Total_Amount, dsLocal.Rows[0]["CurrencyId"].ToString()); ;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            row = row + 4;

            reportUtility.SetSignatureText(ref sheet, row - 1, 1, dsLocal.Rows[0]["AddedBy"].ToString());
            sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

            reportUtility.SetSignatureText(ref sheet, row - 1, 3, dsLocal.Rows[0]["PostedBy"].ToString());
            sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

            sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

            reportUtility.CompanyPlantHeader(ref sheet, colLast, "Opening Balance Journal Voucher", companyId, plantName, null);
            reportUtility.FreezePage(ref sheet, 1, colLast);
            reportUtility.PageAdjustableSetup(ref sheet, 1, row + 3, ExcelPageOrientation.Portrait);
            return workbook;
        }

        public IWorkbook GetExchangeVoucher(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var excelEngine = new ExcelEngine();
            var reportUtility = new ReportUtility();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";
            workbook.Version = ExcelVersion.Excel2016;

            var data = _voucherService.GetJournalData(companyGroupId, companyId, plantId, voucherId);
            if (data.Rows.Count == 0)
                throw new Exception("No data found!");

            // Set report Name
            reportFileName = Convert.ToDateTime(data.Rows[0]["PostingDate"]).ToString("yyMMdd") + " " + data.Rows[0]["VoucherNo"];

            var curCode = data.Rows[0]["CurrencyCode"].ToString();
            var trnCur = data.Rows[0]["TrnCurrency"].ToString();

            reportUtility.SetMasterHeaderText(ref sheet, 5, 1, "Voucher No");
            reportUtility.SetText(ref sheet, 5, 2, data.Rows[0]["VoucherNo"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + 5 + ":" + reportUtility.GetColumnNameForXls(3) + 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 5, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, 5, 5, data.Rows[0]["VoucherDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(5) + 5 + ":" + reportUtility.GetColumnNameForXls(6) + 5].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 6, 1, "Doc Date");
            reportUtility.SetText(ref sheet, 6, 2, data.Rows[0]["DocDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + 6 + ":" + reportUtility.GetColumnNameForXls(3) + 6].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 6, 4, "Doc No");
            reportUtility.SetText(ref sheet, 6, 5, data.Rows[0]["DocRefNo"].ToString());
            sheet[reportUtility.GetColumnNameForXls(5) + 6 + ":" + reportUtility.GetColumnNameForXls(6) + 6].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 7, 1, "Posting Date");
            reportUtility.SetText(ref sheet, 7, 2, data.Rows[0]["PostingDate"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + 7 + ":" + reportUtility.GetColumnNameForXls(3) + 7].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 7, 4, "Fiscal Year");
            reportUtility.SetText(ref sheet, 7, 5, data.Rows[0]["FiscalYearName"] + " (" + data.Rows[0]["PeriodNo"] + ")");
            sheet[reportUtility.GetColumnNameForXls(5) + 7 + ":" + reportUtility.GetColumnNameForXls(6) + 7].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 8, 1, "Narration");
            reportUtility.SetText(ref sheet, 8, 2, data.Rows[0]["Narration"].ToString());
            sheet[reportUtility.GetColumnNameForXls(2) + 8 + ":" + reportUtility.GetColumnNameForXls(3) + 9].Merge();

            reportUtility.SetMasterHeaderText(ref sheet, 8, 4, "Status");
            reportUtility.SetText(ref sheet, 8, 5, data.Rows[0]["Park/Post"].ToString());
            sheet[reportUtility.GetColumnNameForXls(5) + 8 + ":" + reportUtility.GetColumnNameForXls(6) + 8].Merge();

            var col = 1;
            reportUtility.SetHeaderText(ref sheet, 10, col, "GL", 28); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Budget", 15); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Activity", 15); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Party", 18); col++;
            if (curCode != trnCur)
            {
                reportUtility.SetHeaderText(ref sheet, 10, col, "Trn Currency", 7); col++;
                reportUtility.SetHeaderText(ref sheet, 10, col, "Trn Value", 9); col++;
            }
            reportUtility.SetHeaderText(ref sheet, 9, col, curCode, ExcelHAlign.HAlignCenter);
            sheet[9, col, 9, col + 1].Merge();
            reportUtility.SetHeaderText(ref sheet, 10, col, "Debit", 11, ExcelHAlign.HAlignRight); col++;
            reportUtility.SetHeaderText(ref sheet, 10, col, "Credit", 11, ExcelHAlign.HAlignRight);
            var colLast = col;
            var row = 11;

            double _Total_Amount = 0;
            var Row_Total_Start = row;
            for (int n = 0; n < data.Rows.Count; n++)
            {
                col = 1;
                reportUtility.SetText(ref sheet, row, col, data.Rows[n]["AccountCode"] + " - " + data.Rows[n]["GL"]); col++;
                reportUtility.SetText(ref sheet, row, col, data.Rows[n]["BudgetName"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, data.Rows[n]["Activity"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, data.Rows[n]["PartyName"].ToString()); col++;
                if (curCode != trnCur)
                {
                    reportUtility.SetText(ref sheet, row, col, data.Rows[n]["TrnCurrency"].ToString()); col++;
                    reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(data.Rows[n]["Value"])); col++;
                }
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(data.Rows[n]["DrAmount"].ToString()));
                _Total_Amount += Convert.ToDouble(data.Rows[n]["DrAmount"].ToString()); col++;
                reportUtility.SetText(ref sheet, row, col, Convert.ToDouble(data.Rows[n]["CrAmount"].ToString())); col++;
                row++;
            }

            var rowLast = row - 1;
            sheet.Range[reportUtility.GetColumnNameForXls(1) + row + ": " + reportUtility.GetColumnNameForXls(colLast - 2) + row].Merge();
            reportUtility.SetText(ref sheet, row, 1, "Total : ", true);

            sheet.Range[row, colLast - 1].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colLast - 1) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(colLast - 1) + rowLast + ")";
            sheet.Range[row, colLast - 1].NumberFormat = reportUtility.NumberFormatDecimalTwo();
            sheet.Range[row, colLast - 1].CellStyle.Font.Bold = true;
            sheet.Range[row, colLast - 1].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[row, colLast].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colLast) + Row_Total_Start + ":" + reportUtility.GetColumnNameForXls(colLast) + rowLast + ")";
            sheet.Range[row, colLast].NumberFormat = reportUtility.NumberFormatDecimalTwo();
            sheet.Range[row, colLast].CellStyle.Font.Bold = true;
            sheet.Range[row, colLast].BorderAround(ExcelLineStyle.Hair);

            sheet.Range[11, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[11, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
            row += 2;

            reportUtility.SetText(ref sheet, row, 1, "In Word : ", true);

            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(_Total_Amount, data.Rows[0]["CurrencyId"].ToString()); ;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

            sheet.UsedRange.AutofitColumns();
            sheet.UsedRange.CellStyle.Font.Size = 8;
            row = row + 4;

            reportUtility.SetSignatureText(ref sheet, row - 1, 1, data.Rows[0]["AddedBy"].ToString());
            sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

            reportUtility.SetSignatureText(ref sheet, row - 1, 3, data.Rows[0]["PostedBy"].ToString());
            sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

            sheet.Range[row, colLast].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
            reportUtility.SetTextMiddle(ref sheet, row, colLast, "Authorized By", true);

            reportUtility.CompanyPlantHeader(ref sheet, colLast, "Exchange Voucher", companyId,plantId, plantName, null);
            reportUtility.PageAdjustableSetup(ref sheet, 1, row + 3, ExcelPageOrientation.Portrait);
            return workbook;
        }


        public IWorkbook GetFiscalYearBudgetReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearPeriodId)
        {
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsBioDvAC = null;
            DataTable dtBioDvAC = null;
            DataView dvBioDvAC = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataView dvPayDays = null;
            //StringCollection sEmpCodeColl = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            var workbook = oru.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            IWorksheet sheet1 = null;
            // DataView dvWeeklyAbsnt = null;
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            // string freezeRow = "";
            var iActualBudgetAmount = 0;
            var iActualExpenceAmount = 0;
            var iExcessForTheMonth = 0;
            var iShortForTheMonth = 0;
            var iRefNo = 0;

            var totalStanderdBudget = 0.00;
            var totalMonthly = 0.00;
            var totalActual = 0.00;
            var totalExcess = 0.00;
            var totalShort = 0.00;

            try
            {
                objRpt = new clsReport();

                dvPayDays = new DataView();

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region DataSet

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                GetfiscalYearBudget(fiscalYearPeriodId, out dsBioDvAC);

                dtBioDvAC = dsBioDvAC.Tables[0];
                #endregion DataSet

                if (dsBioDvAC.Tables[0].Rows.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    dvBioDvAC = new DataView
                    {
                        Table = dtBioDvAC
                    };
                    if (dvBioDvAC.Count > 0)
                    {
                        sheet1 = workbook.Worksheets[0];
                        sheet1.IsGridLinesVisible = true;
                        xlsRow = 6;

                        // string strEmpCode = "";
                        int iBudgetName = 0;
                        var iStandardAmount = 0;

                        if (dvBioDvAC.Count > 0)
                        {
                            #region ------------------Column Header------------------

                            xlsCol = 1;
                            xlsRow = 5;
                            sheet1.Range[xlsRow, xlsCol].Text = "Fiscal Year Name";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[0]["FiscalYearName"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                            xlsCol = 1;
                            xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Period Name";
                            sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[0]["PeriodName"].ToString().Trim();
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                            //xlsCol = 1;
                            //xlsRow += 1;
                            //sheet1[xlsRow, xlsCol].Text = "Ref No";
                            //sheet1[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[0]["RefNo"].ToString().Trim();
                            //sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            //sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                            //xlsCol = 1;
                            //xlsRow += 1;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                            xlsCol = 1;

                            xlsRow += 1;
                            iBudgetName = xlsCol;

                            sheet1.Range[xlsRow, iBudgetName].Text = "Budget Name";
                            sheet1.Range[xlsRow, iBudgetName].ColumnWidth = 30;
                            sheet1.Range[xlsRow, iBudgetName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, iBudgetName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            // sheet1.Range[xlsRow, iBudgetName, xlsRow, iBudgetName + 1].Merge();

                            // xlsRow += 1;
                            xlsCol += 1;

                            iRefNo = xlsCol;

                            sheet1.Range[xlsRow, iRefNo].Text = "Ref No";
                            sheet1.Range[xlsRow, iRefNo].ColumnWidth = 9;
                            sheet1.Range[xlsRow, iRefNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, iRefNo].VerticalAlignment = ExcelVAlign.VAlignCenter;


                            //xlsCol += 2;
                            xlsCol += 1;
                            iStandardAmount = xlsCol;
                            sheet1.Range[xlsRow, iStandardAmount].Text = "Standard";
                            sheet1.Range[xlsRow, iStandardAmount].ColumnWidth = 16;
                            sheet1.Range[xlsRow, iStandardAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, iStandardAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            // sheet1.Range[xlsRow, iStandardAmount, xlsRow, iStandardAmount + 1].Merge();

                            xlsCol += 1;
                            iActualBudgetAmount = xlsCol;
                            sheet1.Range[xlsRow, iActualBudgetAmount].Text = "Monthly";
                            sheet1.Range[xlsRow, iActualBudgetAmount].ColumnWidth = 16;
                            sheet1.Range[xlsRow, iActualBudgetAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, iActualBudgetAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iActualExpenceAmount = xlsCol;
                            sheet1.Range[xlsRow, iActualExpenceAmount].Text = "Actual";
                            sheet1.Range[xlsRow, iActualExpenceAmount].ColumnWidth = 19;
                            sheet1.Range[xlsRow, iActualExpenceAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, iActualExpenceAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iExcessForTheMonth = xlsCol;
                            sheet1.Range[xlsRow, iExcessForTheMonth].Text = "Excess";
                            sheet1.Range[xlsRow, iExcessForTheMonth].ColumnWidth = 19;
                            sheet1.Range[xlsRow, iExcessForTheMonth].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, iExcessForTheMonth].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            iShortForTheMonth = xlsCol;
                            sheet1.Range[xlsRow, iShortForTheMonth].Text = "Short";
                            sheet1.Range[xlsRow, iShortForTheMonth].ColumnWidth = 19;
                            sheet1.Range[xlsRow, iShortForTheMonth].HorizontalAlignment = ExcelHAlign.HAlignRight;
                            sheet1.Range[xlsRow, iShortForTheMonth].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow - 1, iExcessForTheMonth].Text = "Monthly Vs Actual";
                            sheet1.Range[xlsRow - 1, iExcessForTheMonth, xlsRow - 1, iShortForTheMonth].Merge();
                            sheet1.Range[xlsRow - 1, iExcessForTheMonth, xlsRow - 1, iShortForTheMonth].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow - 1, iExcessForTheMonth, xlsRow - 1, iShortForTheMonth].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow - 1, iExcessForTheMonth, xlsRow - 1, iShortForTheMonth].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow - 1, iExcessForTheMonth, xlsRow - 1, iShortForTheMonth].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow - 1, iExcessForTheMonth, xlsRow - 1, iShortForTheMonth].BorderAround(ExcelLineStyle.Hair);


                            //sheet1.Range[xlsRow, 1, xlsRow, xlsCol+1].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                            //sheet1.Range[xlsRow, 1, xlsRow, xlsCol+1].BorderAround(ExcelLineStyle.Hair);
                            //sheet1.Range[xlsRow, 1, xlsRow, xlsCol+1].BorderInside(ExcelLineStyle.Hair);
                            //sheet1.Range[xlsRow, 1, xlsRow, xlsCol+1].CellStyle.Font.Bold = true;

                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                            //endXlsCol = iStandardAmount+1;
                            endXlsCol = iShortForTheMonth;
                            #endregion ------------------Column Header------------------

                            #region ----------------------Data-----------------------
                            for (int i = 0; i < dvBioDvAC.Count; i++)
                            {
                                xlsRow += 1;
                                sheet1.Range[xlsRow, iBudgetName].Text = dvBioDvAC[i]["BudgetName"].ToString();
                                sheet1.Range[xlsRow, iBudgetName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, iBudgetName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //sheet1.Range[xlsRow, iBudgetName, xlsRow, iBudgetName + 1].Merge();

                                sheet1.Range[xlsRow, iRefNo].Text = dvBioDvAC[i]["RefNo"].ToString();
                                sheet1.Range[xlsRow, iRefNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, iRefNo].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                sheet1.Range[xlsRow, iStandardAmount].Text = dvBioDvAC[i]["StandardAmount"].ToString();
                                sheet1.Range[xlsRow, iStandardAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, iStandardAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                // sheet1.Range[xlsRow, iStandardAmount, xlsRow, iStandardAmount + 1].Merge();
                                totalStanderdBudget += clsStaticInfo.dbl(dvBioDvAC[i]["StandardAmount"].ToString());

                                sheet1.Range[xlsRow, iActualBudgetAmount].Number = clsStaticInfo.dbl(dvBioDvAC[i]["ActualBudgetAmount"].ToString());
                                sheet1.Range[xlsRow, iActualBudgetAmount].NumberFormat = oru.GetDynamicDecimalPlace(2);
                                sheet1.Range[xlsRow, iActualBudgetAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, iActualBudgetAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                totalMonthly += clsStaticInfo.dbl(dvBioDvAC[i]["ActualBudgetAmount"].ToString());

                                sheet1.Range[xlsRow, iActualExpenceAmount].Number = clsStaticInfo.dbl(dvBioDvAC[i]["ActualExpAmount"].ToString());
                                sheet1.Range[xlsRow, iActualExpenceAmount].NumberFormat = oru.GetDynamicDecimalPlace(2);
                                sheet1.Range[xlsRow, iActualExpenceAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, iActualExpenceAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                totalActual += clsStaticInfo.dbl(dvBioDvAC[i]["ActualExpAmount"].ToString());

                                sheet1.Range[xlsRow, iExcessForTheMonth].Number = clsStaticInfo.dbl(dvBioDvAC[i]["ExcessAmount"].ToString());
                                sheet1.Range[xlsRow, iExcessForTheMonth].NumberFormat = oru.GetDynamicDecimalPlace(2);
                                sheet1.Range[xlsRow, iExcessForTheMonth].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, iExcessForTheMonth].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                totalExcess += clsStaticInfo.dbl(dvBioDvAC[i]["ExcessAmount"].ToString());

                                sheet1.Range[xlsRow, iShortForTheMonth].Number = clsStaticInfo.dbl(dvBioDvAC[i]["ShortAmount"].ToString());
                                sheet1.Range[xlsRow, iShortForTheMonth].NumberFormat = oru.GetDynamicDecimalPlace(2);
                                sheet1.Range[xlsRow, iShortForTheMonth].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, iShortForTheMonth].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                totalShort += clsStaticInfo.dbl(dvBioDvAC[i]["ShortAmount"].ToString());

                                #endregion ----------------------Data-----------------------

                                #region Line Setup

                                //sheet1.Range[xlsRow, 1, xlsRow, iStandardAmount + 1].BorderInside(ExcelLineStyle.Hair);
                                //sheet1.Range[xlsRow, 1, xlsRow, iStandardAmount + 1].BorderAround(ExcelLineStyle.Hair);
                                //sheet1.Range[xlsRow, 1, xlsRow, iStandardAmount + 1].WrapText = true;

                                sheet1.Range[xlsRow, 1, xlsRow, iShortForTheMonth].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, iShortForTheMonth].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, iShortForTheMonth].WrapText = true;

                                #endregion Line Setup
                            }
                            xlsRow++;
                            sheet1.Range[xlsRow, iRefNo].Text = "Total";
                            sheet1.Range[xlsRow, iRefNo + 1].Number = totalStanderdBudget;
                            sheet1.Range[xlsRow, iRefNo + 2].Number = totalMonthly;
                            sheet1.Range[xlsRow, iRefNo + 3].Number = totalActual;
                            sheet1.Range[xlsRow, iRefNo + 4].Number = totalExcess;
                            sheet1.Range[xlsRow, iRefNo + 5].Number = totalShort;
                            sheet1.Range[xlsRow, iRefNo, xlsRow, iRefNo + 5].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, iRefNo, xlsRow, iRefNo + 5].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, iRefNo, xlsRow, iRefNo + 5].BorderAround(ExcelLineStyle.Hair);


                        }


                        #region ******************Report Header******************

                        xlsRow = 1;
                        xlsCol = 1;
                        FactoryName = string.Empty;

                        string FactoryAddress = string.Empty;

                        if (dsCmp.Tables[0].Rows.Count > 0)
                        {
                            CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                        }
                        else
                        {
                            CmpName = "";
                        }
                        sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                        sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                        sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;
                        if (dsFactory.Tables[0].Rows.Count > 0)
                        {
                            //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                            FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                        }
                        else
                        {
                            FactoryName = "";
                        }
                        sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                        sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 25;
                        sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;
                        if (dsFactory.Tables[0].Rows.Count > 0)
                        {
                            FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                        }
                        else
                        {
                            FactoryAddress = "";
                        }
                        sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                        //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 15;
                        sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = "Monthly Budget";
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                        sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                        sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        #endregion ******************Report Header******************

                        #region Freeze Panes

                        sheet1.IsDisplayZeros = false;
                        sheet1.UsedRange["A8"].FreezePanes();
                        //sheet1.FirstVisibleColumn = 1;
                        //sheet1.FirstVisibleRow = 10;

                        #endregion Freeze Panes

                        #region UsedRange Alignment

                        sheet1.UsedRange.WrapText = true;
                        sheet1.UsedRange.CellStyle.Font.Size = 8;
                        sheet1.Range["A1"].CellStyle.Font.Size = 14;
                        sheet1.Range["A2"].CellStyle.Font.Size = 10;
                        sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                        #endregion UsedRange Alignment

                        #region Page Setup
                        sheet1.PageSetup.TopMargin = 0.5;
                        sheet1.PageSetup.BottomMargin = 0.7;
                        sheet1.PageSetup.PrintTitleRows = "$1:$14";
                        sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                        sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                        sheet1.PageSetup.LeftMargin = 0.5;
                        sheet1.PageSetup.RightMargin = 0.2;
                        sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                        sheet1.PageSetup.FitToPagesTall = 0;
                        sheet1.PageSetup.FitToPagesWide = 1;
                        sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                        //sheet1.IsDisplayZeros = false;

                        sheet1.Name = "Monthly Budget";

                        #endregion Page Setup

                    }

                    return workbook;
                }
                else
                {
                    Exception ex = new Exception("No data found...");
                    //System.Exception ex = new Exception("No data found...[For Employee Code : '" + txtEmployeeCode.Text+"']");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsBioDvAC = null;
                dvBioDvAC = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }


        }

        public void GetfiscalYearBudget(string fiscalYearPeriodId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select FY.FiscalYearCode,FY.FiscalYearName,FYP.Id,FYP.PeriodName,BM.RefNo,B.UserName BudgetName, ISNULL(ABD.StandardAmount,0) StandardAmount, ISNULL(ABD.ActualAmount,0) ActualBudgetAmount, ISNULL(V.ActualAmount,0) ActualExpAmount,
                            ExcessAmount=CASE WHEN ISNULL((ABD.StandardAmount-V.ActualAmount),0) <0 THEN ABS(ISNULL((ABD.StandardAmount-V.ActualAmount),0)) ELSE 0 END,
                            ShortAmount=CASE WHEN ISNULL((ABD.StandardAmount-V.ActualAmount),0) >0 THEN ISNULL((ABD.StandardAmount-V.ActualAmount),0) ELSE 0 END
                            from MST.AnnualBudget AB
                            left join mst.AnnualBudgetDetail ABD ON ABD.AnnualBudgetId=AB.Id
                            LEFT JOIN MST.BudgetMaster BM ON BM.Id=AB.BudgetMasterId
                            LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                            LEFT JOIN SCS.FiscalYear FY ON FY.Id=AB.FiscalYearId
                            LEFT JOIN SCS.FiscalYearPeriod FYP ON FYP.Id=ABD.FiscalYearPeriodId
                            LEFT JOIN (SELECT VD.BudgetMasterId,SUM(ISNULL(VDC.DrAmount,0))-SUM(ISNULL(VDC.CrAmount,0)) ActualAmount
                            FROM TRN.VoucherDetail VD 
                            JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id where VD.FiscalYearPeriodId='" + fiscalYearPeriodId + @"' GROUP BY VD.BudgetMasterId) as V ON V.BudgetMasterId=AB.BudgetMasterId
                             WHERE FYP.Id='" + fiscalYearPeriodId + @"'";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function


    }
}