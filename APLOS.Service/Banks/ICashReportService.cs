using Library.Model.Enums;
using Syncfusion.XlsIO;
using System;

namespace Library.Service.Banks
{
    public interface ICashReportService
    {
        IWorkbook GetCashJVReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);

        IWorkbook GetCashJournalReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);

        IWorkbook GetCashOpeningBalanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId, bool isCompanyCurrency);

        IWorkbook GetCashLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string cashMasterId, string fromDate, string toDate);

        IWorkbook GetCashBookReport(string companyGroupId, string companyId, string plantId, string plantName, string cashMasterId, string fromDate, string toDate);
        
        IWorkbook GetAdvanceCashBookReport(string companyGroupId, string companyId, string plantId, string plantName, string cashMasterId, string fromDate, string toDate);

        IWorkbook GetCashReceiptPaymentReport(string companyGroupId, string companyId, string plantId, string plantName, string cashMasterId, string fromDate, string toDate);

        IWorkbook GetMontlyExpensesAndAssetWorkBook(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, DateTime fromDate, DateTime toDate);


    }
}