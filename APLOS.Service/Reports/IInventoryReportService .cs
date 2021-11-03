using Syncfusion.XlsIO;

namespace Library.Service.Reports
{
    public interface IInventoryReportService
    {
        IWorkbook GetInventoryReport(string companyGroupId, string companyId, string plantId, string materialId, string articleId);
    }
}