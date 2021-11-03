using Library.Model.Enums;
using Syncfusion.XlsIO;

namespace Library.Service.Banks
{
    public interface IBankReportService
    {

        IWorkbook GetBankJournalReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);

        IWorkbook GetBankOpeningBalanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId, bool isCompanyCurrency);

        IWorkbook GetBankLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate);
        IWorkbook GetBankReconcileReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate);
        IWorkbook xGetBankLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate);

        IWorkbook GetBankBookReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate);


        IWorkbook GetPaymentByBankReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);
    }
}