using Syncfusion.XlsIO;

namespace Library.MaterialManagement.Reports
{
    public interface ISalesReportService
    {
        IWorkbook GetSalesReport(out string reportFileName, string companyGroupId, string companyId,string plantName, string plantId, string salesId);
        void GetSalesWordReportService(string companyGroupId, string companyId, string plantId, string UserId, string salesId);
       void LocalTaxInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string salesId);
        void CommercialInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string salesId);
    }
}