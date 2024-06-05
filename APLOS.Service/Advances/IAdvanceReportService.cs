using Library.Model.Enums;
using Syncfusion.XlsIO;
using System.Data;

namespace Library.Service.Advances
{
    public interface IAdvanceReportService
    {
        IWorkbook GetAdvanceReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);
        IWorkbook GetAdvanceSetOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string reportName, SourceType sourceType);

        IWorkbook GetVendorAdvanceWriteOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);

        IWorkbook GetCustomerAdvanceWriteOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);

        IWorkbook GetEmployeeAdvanceReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);
        IWorkbook GetEmployeeAdvanceReportPortal(string companyGroupId, string companyId, string plantId, string plantName, string employeeAdvanceRequisitionId);

        IWorkbook GetInvoiceChargeReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string reportName, SourceType sourceType);
        IWorkbook GetEmployeeAdvanceWriteOffReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);

        IWorkbook GetInterTransactionReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);
        IWorkbook GetInterTransactionVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);
        string VendorAdvanceReport(string companyGroupId, string companyId, string plantId, SourceType sourceType, string SheetName);
        IWorkbook GetEmployeeAdvanceHRReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);
    }
}