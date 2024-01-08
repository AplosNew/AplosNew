using Library.Model.Enums;
using Syncfusion.XlsIO;
using System;

namespace Library.Service.Vouchers
{
    public interface IVoucherReportService
    {

        IWorkbook GetGLVoucher(out ExcelEngine excelEngine, string masterId);

        IWorkbook GetCoa(out ExcelEngine excelEngine, string masterId);

        IWorkbook GetGLDateWise(out ExcelEngine excelEngine, string masterId, string fromDate, string toDate);


        IWorkbook GetIncomeStatementReport(string companyId,  string plantId,string plantName, string date, string[] parallelCurrencies, bool isBudgetLevel, bool isActivityLevel);
        IWorkbook GetIncomeStatementYearClosedReport(string companyId,  string plantId,string plantName, string fiscalYearCloseId, string fiscalYearName, bool isBudgetLevel, bool isActivityLevel);

        IWorkbook GetFiscalYearBudgetReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearPeriodId);


        IWorkbook GetGeneralVoucher(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);
        IWorkbook GetOBAdvanceJournalVoucher(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string openingBalanceId);
        IWorkbook GetExchangeVoucher(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);

        IWorkbook GetIncomeStatementReportDateWise(string companyId, string PlantId, string plantName, string fromDate, string toDate, string[] parallelCurrencies, bool isBudgetLevel, bool isActivityLevel);
        IWorkbook GetBalanceSheetReportDateWise(string companyId, string plantName, string fromDate, string toDate);

        IWorkbook GetEntityWiseExpenseAndEarningReportDateWise(string companyId, string PlantId, string plantName, string fromDate, string toDate, string entityId, string entity, string[] parallelCurrencies);
        IWorkbook EntityWiseExpenseAndEarningreportDateWiseActivityLevel(string companyId, string PlantId, string plantName, string fromDate, string toDate, string entityId, string entity, string[] parallelCurrencies);
    }
}