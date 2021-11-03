using Syncfusion.XlsIO;

namespace Library.Service.Organizations
{
    public interface IPositionReportService
    {
        IWorkbook PositionReport(string companyGroupId);
    }
}