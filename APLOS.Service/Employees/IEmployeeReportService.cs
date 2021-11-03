using System.Data;
using Syncfusion.XlsIO;
using Library.Model.Enums;

namespace Library.Service.Employees
{
    public interface IEmployeeReportService
    {
        IWorkbook GetEmployeeOpeningBalanceLedgerReport(string companyGroupId, string companyId, string plantId, string plantName, string fiscalYearId);


        IWorkbook GetEmployeeExpenseBookingReport(string companyGroupId, string companyId, string plantId, string plantName, string employeeId, string fromDate, string toDate);

        IWorkbook GetAssetRegisterExpenseBookingReport(string companyGroupId, string companyId, string plantId, string plantName, string fixedAssetRegisterId, string fromDate, string toDate);

        IWorkbook GetEmployeePayableExpenseBookingReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);

        IWorkbook GetEmployeePayment(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);
    

        IWorkbook GetExpensesBookingReport(string companyGroupId, string companyId, string plantId, string plantName, string expenseBookingId);
        DataTable CompanyHeader(string companyId);
        DataTable GetEmployeePayablePayment(string companyId, string voucherId);
        IWorkbook GetCashExpenseReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, SourceType sourceType);
    }
}