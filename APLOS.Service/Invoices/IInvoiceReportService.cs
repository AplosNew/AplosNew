using Syncfusion.XlsIO;

namespace Library.Service.Invoices
{
    public interface IInvoiceReportService
    {
        IWorkbook GetCustomerInvoiceReceive(string companyGroupId, string companyId, string plantId, string plantName, string voucherId);


       

        IWorkbook GetCustomerInvoiceReport (out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);

       
        IWorkbook GetCustomerInvoiceReceiptReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType);
        IWorkbook GetCustomerInvoiceReceiptGovtSubsidyReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType);
        IWorkbook GetVendorInvoiceReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId,string sourceType);
        IWorkbook GetIncentiveReceivableInvoiceReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);

        IWorkbook GetInvoiceOverheadReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);
        IWorkbook GetVendorPaymentReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);

        IWorkbook GetCustomerInvoiceSettlementReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string bankJournalId);
        IWorkbook GetSettlementGainLossReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);
        IWorkbook GetPartyReconciliationReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherWriteOffId);
        IWorkbook GetPurchaseLCChargesReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);
        IWorkbook DocumentAcceptanceVoucher(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId);
        IWorkbook GetInvoiceChargesReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType);
    }
}