using Library.Model.Enums;
using Syncfusion.XlsIO;
using System.Collections.Generic;

namespace Library.Service.Banks
{
    public interface IBankReportService
    {

        IWorkbook GetBankJournalReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);

        IWorkbook GetBankOpeningBalanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId, bool isCompanyCurrency);

        IWorkbook GetBankLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate, bool extended);
        IWorkbook GetBankReconcileReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate);
        IWorkbook xGetBankLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate);

        IWorkbook GetBankBookReport(string companyGroupId, string companyId, string plantId, string plantName, string bankMasterId, string fromDate, string toDate);


        IWorkbook GetPaymentByBankReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);
        void CRReconcileReport(string BankMasterID, string fromDate, string toDate);
        void DRReconcileReport(string BankMasterID, string fromDate, string toDate, string cutOffDate);
        void DRReconcilePendingReport(string bankMasterId, string fromDate, string toDate);
        void CRReconcilePendingReport(string bankMasterId, string fromDate, string toDate);
    }
}