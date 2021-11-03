using Library.Model.Enums;
using Syncfusion.XlsIO;

namespace Library.Service.Advances
{
    public interface IAdjustmentNoteReportService
    {
        IWorkbook GetDebitNoteReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);
        IWorkbook GetCreditNoteReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);

        IWorkbook CreditNoteSetOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);
        //object DebitNoteSetOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType debitNoteSetOff);


        //IWorkbook CreditNoteSetOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);

        IWorkbook DebitNoteSetOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);
    }
}