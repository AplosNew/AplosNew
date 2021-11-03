#region Using

using Library.Model.Invoices;
using Library.Service.Core;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Invoices
{
    public interface ITaxPaymentService : IService<InvoiceTaxWriteOff>
    {
        List<Dictionary<string, object>> GetInvoiceTaxPayableList(string companyGroupId, string companyId, string taxCategoryId, DateTime fromDate, DateTime todate, string partyType, string partyId, string partyPlantId);
        List<Dictionary<string, object>> GetTaxPaymentDataList(string column, string value, string companyId, string plantId);
        void InsertTaxPayment(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        IWorkbook GetTaxPayableReport(string companyGroupId, string companyId, string plantId, string plantName, string taxCategoryId, string fromDate, string toDate);
    }
}