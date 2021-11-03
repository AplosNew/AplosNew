using Library.Model.Enums;
using Library.Model.Parties;
using Syncfusion.XlsIO;

namespace Library.Service.Finances
{
    public interface ILoanReportService
    {
        IWorkbook GetLoanReport(out string reportFileName, string companyGroupId, string companyId, string plantName, string plantId, string voucherId, string sourceType);
        IWorkbook GetLoanWriteOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType);
        IWorkbook GetLoanLedgerReport(string companyGroupId, string companyId, string plantId,string plantName, TransactionType transactionType, string voucherId, string financingId);
        IWorkbook GetLoanInterestPayableReport(out string reportFileName, string companyGroupId, string companyId, string plantId,string plantName, string voucherId, string sourceType);
        IWorkbook GetLoanRegisterLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, TransactionType transactionType, string voucherId, string financingId);
    }
}