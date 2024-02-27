#region Using

using Library.Core;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Service.Core;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface IMasterOrderService : IService<MasterOrder>
    {
        Dictionary<string, object> GetWeekendbyBuyer(string buyerId);
        string GetDelivaryDate(string year, int weekNo, string buyerId);
        object GetOrderDateSetting(string shipmentModeId, string buyerId);
        GridModel QueryIdependent(GridParameter parameters, string companyId);
        void InsertOrUpdate(MasterOrder entity);
        void Insert(MasterOrder entity, List<MasterOrderTNA> taskList, UserRemarksControl userRemarksControl);
        IEnumerable<object> GetTaskList(string buyerId, string buyerDepartmentId, string buyerDivisionId, string moId);
        void InsertOrUpdateSplitSOGraph(string masterItemId, SalesOrderMaster salesOrderMaster);
        IEnumerable<object> GetSpecialTaxList(string plantId);
        //void GetMasterOrderReport(string companyId, string plantId, string masterOrderId);

        void GetProformaInvoiceReportService(string companyId, string plantId, string masterOrderId);
        //DataTable LoadOrderMasterTax(string OrderMasterID);
        //DataTable LoadOrderMasterItems(string OrderMasterID);
        //DataTable LoadOrderMaster(string companyId, string OrderMasterID);
        void UpdateSOGraph(string masterItemId, SalesOrderMaster salesOrderMaster, IEnumerable<SalesOrderTax> taxCategoryList);
        void CheckSOGraph(SalesOrderMaster salesOrderMaster);
        void ApproveSOGraph(SalesOrderMaster salesOrderMaster);
       // IEnumerable<object> GetDepartmentPersonCbo(string plantId, string partyAccountGroupId, string partyId);

        GridModel Query(GridParameter parameters, string companyId);
        IEnumerable<object> GetList(string companyId, string column, string value);
        IEnumerable<object> GetMasterOrderList(string companyId, string plantId);
        //IEnumerable<object> GetDepartmentPersonList(string plantId, string partyAccountGroupId, string partyId, bool flag);

        IEnumerable<object> GetResponsiblePersonList(string masterId);

        IEnumerable<object> GetArticleCodeList(string materialMasterId, string articleCode);

        GridModel GetCompanyPartyList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string customerVendor);

        IEnumerable<object> GetItemsData(string masterOrderId);
        IEnumerable<object> GetMasterItemList(string masterOrderId);
        IEnumerable<object> GetMasterItemForApproveList(string masterOrderId,string empId);
        IEnumerable<object> GetAttributeListByMaterialMasterId(string materialMasterId);

        IEnumerable<object> GetOrderAttributeListByMasterId(string masterItemId, string materialMasterId);

        IEnumerable<object> GetSOList(string masterItemId);
        IEnumerable<object> GetpackingTypeList(string SOId,string PackingType);
        IEnumerable<object> GetFirstSkuSalesOrderId(string salesOrderId);

        IEnumerable<object> GetSecondSkuSalesOrderId(string salesOrderId);

        IEnumerable<object> GetThirdSkuSalesOrderId(string salesOrderId);

        IEnumerable<object> GetCharacteristicsByMaterialMasterId(string materialMasterId);

        IEnumerable<object> GetChValueCbo(string materialId);

        IEnumerable<object> GetChValueCboByMaterialId(string materialId);
        IEnumerable<object> GetTaxCategoryList(string companyGroupId, string masterOrderId, string plantId, string hsnCodeId, string specialTaxId, string PODate);

        IEnumerable<object> GetSalesOrderTaxCategoryList(string salesOrderId);

        GridModel GetEmployeeListResponsible(GridParameter parameters, string companyId, string plantId, string partyAccountGroupId, string partyId);
        GridModel GetPreparedEmployeeList(GridParameter parameters, string plantId, string employeeId);
        void Update(MasterOrder entity, string masterId, IEnumerable<MasterOrderResPerson> personList, IEnumerable<MasterOrderItem> itemList, UserRemarksControl userRemarksControl);

        void InsertOrUpdateGraph(string masterItemId, IEnumerable<MasterOrderAttributeValue> attributeValueList);

        void DeleteGraph(string id);

        void InsertOrUpdateSOGraph(string masterItemId, SalesOrderMaster salesOrderMaster);

        void DeleteSOGraph(string masterItemId, SalesOrderMaster salesOrderMaster);

        void InsertOrUpdateCharacteristics(IEnumerable<SalesOrderCharacteristicsViewModel> entities, int listLength, string soId);

        void InsertOrUpdateSalesOrderTax(string salesOrderId, IEnumerable<SalesOrderTax> salesOrderTaxList);

        void DeleteItem(string id);

        void DeleteSO(string id);

        void DeleteFirstSku(string id);
        object GetSOBookedQtyAndLevel(string salesOrderId);
        object GetPOBookedQtyAndLevel(string salesOrderId);

        void UpdateSODateGraph(SalesOrderMaster salesOrderMaster);
        void UpdateSODate(SalesOrderMaster salesOrderMaster);
        void UpdateSORate(SalesOrderMaster salesOrderMaster);
        void UpdateSOQTY(SalesOrderMaster salesOrderMaster);
        void UpdateSOStatus(SalesOrderMaster salesOrderMaster);
    }
}