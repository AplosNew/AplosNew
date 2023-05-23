using Library.Core;
using Library.Model.SalesManagements;
using Library.ViewModel.SalesManagements;
using Library.ViewModel.Vouchers;
using Syncfusion.XlsIO;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.SalesManagements
{
    public interface ISalesService
    {
        IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string PODate);
        void Delete(string Id);
        IEnumerable<object> GetMasterOrderDataByMasterOrderId(string companyId, string masterOrderId, string masterOrderItemId, string salesId);
        object GetMasterOrderIdBySalesId(string salesId);
        GridModel GetMasterOrderSalesList(GridParameter parameters, string companyGroupId, string companyId);
        GridModel GetMaterialSalesList(GridParameter parameters, string companyGroupId, string companyId);

        List<Dictionary<string, object>> GetSalesServiceData(string companyGroupId, string companyId, string plantId, string salesId);
        List<Dictionary<string, object>> GetSalesTaxData(string companyGroupId, string companyId, string plantId, string salesId);
        List<Dictionary<string, object>> GetSalesServiceTaxData(string companyGroupId, string companyId, string plantId, string salesId);
        GridModel GetSalesPendingList(GridParameter parameters, string companyGroupId, string companyId);
        void Insert(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesServiceViewModel> salesServiceVMList);
        void Update(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesServiceViewModel> salesServiceVMList);
        void DeleteTaxRow(string Id);
        void DeleteServiceTaxRow(string Id);
        void DeleteSalesMaterial(string Id);
        void DeleteSalesService(string Id);
        void SalesInvoicePost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> salesJVDetail, IEnumerable<SalesMaterialViewModel> salesMaterialDetailGLList, IEnumerable<SalesServiceViewModel> salesServiceDetailGLList);
        void MasterOrderSalesInsert(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesOrderItem> selectedMasterOrderList, IEnumerable<SalesServiceViewModel> salesServiceVMList);
        void MasterOrderSalesPost(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesMaterialViewModel> salesMaterialDetailGLList, IEnumerable<SalesServiceViewModel> salesServiceDetailGLList);
        void MasterOrderSalesUpdate(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesOrderItem> selectedMasterOrderList, IEnumerable<SalesServiceViewModel> salesServiceVMList);

        void PackingInvoiceInsert(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesPacking> selectedMasterOrderList, IEnumerable<SalesServiceViewModel> salesServiceVMList, DataSet dsDetail, DataSet dsHistory, DataSet dsItemScanData);
        void PackingInvoiceUpdate(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesPacking> selectedMasterOrderList, IEnumerable<SalesServiceViewModel> salesServiceVMList, DataSet dsItemScanData);
        void PackingSalesPost(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList
           , IEnumerable<SalesMaterialViewModel> salesMaterialDetailGLList
            , IEnumerable<SalesServiceViewModel> salesServiceDetailGLList
            , SalesPacking packing, IEnumerable<SalesMaterialViewModel> PackingDetailVMList, string packingVoucherTypeId);


        void DeleteSale(string invoiceId, string voucherId);
        void DeleteMasterOrderSalePost(string companyId, string plantId, string salesId, string voucherId, string deletedRemarks);
    }
}