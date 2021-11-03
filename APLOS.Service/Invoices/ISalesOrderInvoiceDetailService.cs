using Library.Core;
using Library.Model.Productions.QueryModel;
using Library.Model.Productions.SalesOrderInvoice;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Invoices
{
    public interface ISalesOrderInvoiceDetailService : IService<SalesOrderInvoiceDetail>
    {
        GridModel GetPLDetailSearch(GridParameter parameters, string CustomerId, string plantid);

        GridModel GetPLHeadSearch(GridParameter parameters, string EntityId, string CustomerId, string plantid);

        void InitDetail(string MasterId, SalesOrderInvoiceDetail from_ui, out SalesOrderInvoiceDetail from_db);

        IEnumerable<SalesOrderInvoiceDetail> GetDetailList(string SalesOrderInvoicePackingListId);

        IEnumerable<SalesOrderInvoiceDetail> GetDetailListByInvoiceMaster(string InvoiceMasterId);

        IEnumerable<SalesOrderInvoiceDetail> SalesOrderInvoiceDetail(string SalesOrderInvoiceMasterId, string SalesOrderInvoicePackingListId);

        IEnumerable<MaterialPackedAndInvoiced> Get_Invoiced_Material_Edit_SetQty(string SalesOrderInvoicePackingListId);
    }
}