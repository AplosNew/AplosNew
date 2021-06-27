using Library.Core;
using Library.Model.Productions.SalesOrderInvoice;
using Library.Service.Core;
using System.Collections.Generic;

namespace Library.Service.Invoices
{
    public interface ISalesOrderInvoiceMasterService : IService<SalesOrderInvoiceMaster>
    {
        GridModel GetMasterList(GridParameter parameters, string plantId, string entityid);

        GridModel GetFileInfo(GridParameter parameters, string entityid);

        //SalesOrderInvoiceMaster GetMasterById(string MasterId);
        //void SaveMaster(SalesOrderPackingListMaster master, SalesOrderPackingListDetail detail, out string MasterId);
        void SaveMaster(SalesOrderInvoiceMaster uimaster, out string MasterID);

        void DeleteMaster(string Id);

        IEnumerable<object> GetSalesType();

        void SaveDetailList(string InvoiceMasterId, SalesOrderInvoicePackingList ui_detail, SalesOrderInvoiceDetail ui_pdlist);

        IEnumerable<object> GetInvoiceMaster(string id);

        void DeleteDetailSingle(string InvoiceMasterId, string InvoicePackingListId);

        IEnumerable<object> GetBaseLineDateSetting(string PaymentTermId);
    }
}