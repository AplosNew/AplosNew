using Syncfusion.XlsIO;

namespace Library.Service.Reports
{
    public interface ISecurityDepositReportService
    {
        IWorkbook GetSecurityDepositTakenReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);
    }
}