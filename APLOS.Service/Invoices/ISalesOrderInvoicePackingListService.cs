using Library.Model.Productions.SalesOrderInvoice;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Invoices
{
    public interface ISalesOrderInvoicePackingListService : IService<SalesOrderInvoicePackingList>
    {
        // void InitDetail(string MasterId, IEnumerable<SalesOrderInvoicePackingList> from_ui, out List<SalesOrderInvoicePackingList> from_db);

        void InitInvoicePackingList(SalesOrderInvoicePackingList from_ui, out SalesOrderInvoicePackingList from_db);

        IEnumerable<object> GetInvoicePackingListHead(string invoicemasterid);

        // void DeleteDetail(string masterid);
        IEnumerable<SalesOrderInvoicePackingList> GetDetailList(string SalesOrderInvoiceMasterId);

        void DelInvoiceDetail(string ipid, out IEnumerable<SalesOrderInvoiceDetail> from_db);

        void DelInvoicePackingList(string id, out SalesOrderInvoicePackingList from_db);

        // IEnumerable<SalesOrderInvoicePackingList> GetDetailList(string SalesOrderInvoiceMasterId);
    }
}