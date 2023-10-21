using Syncfusion.XlsIO;

namespace Library.Service.Finances
{
    public interface IInvestmentReportService
    {
        IWorkbook GetInvestmentReport(out string reportFileName, string companyGroupId, string companyId, string PlantName, string plantId, string voucherId, string sourceType);
        IWorkbook GetInvestmentWriteOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType);
    }
}