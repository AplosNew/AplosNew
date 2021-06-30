using Syncfusion.XlsIO;

namespace Library.MaterialManagement.Reports
{
    public interface ISecurityDepositReportService
    {
        IWorkbook GetSecurityDepositTakenReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);
    }
}