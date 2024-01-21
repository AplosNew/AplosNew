using Syncfusion.XlsIO;

namespace Library.MaterialManagement.Reports
{
    public interface ISalesReportService
    {
        IWorkbook GetSalesReport(out string reportFileName, string companyGroupId, string companyId,string plantName, string plantId, string salesId);
        void GetSalesWordReportService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void GetLotWiseTaxInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
       void LocalTaxInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
       void SalesReturnService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesReturnId);
        void LocalTaxInvoiceWithProductDetailService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
       void LocalTaxInvoiceWithoutSKUService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void CommercialInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void LRDraftService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void BillofExchange(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void CertificateofOrigin(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void BeneficiaryCertificate(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void InsuranceCoverLetter(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void ANNEXUREReport(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void BankLatter(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId, string BankName);
        void SalesInvoiceService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void GetLotWiseTaxInvoiceServiceReporttoMail(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
        void CommercialInvoicePackingListService(string companyGroupId, string companyId, string plantId, string UserId, string Name, string salesId);
    }
}