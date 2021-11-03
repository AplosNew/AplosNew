#region Using

using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.ViewModel.OrderManagements;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.OrderManagements
{
    public interface ILineEmployeeAssignService : IService<LineEmployeeAssign>
    {
        void InsertOrUpdateGraph(string date, string line,IEnumerable<LineEmployeeAssign> entities, IEnumerable<LineEmployeeAssign> tempEntities);

        void UpdateGraph(IEnumerable<LineProductionOperationBookingViewModel> entities);

        void DeleteGraph(string key);
        IEnumerable<object> QueryGraph(string date, string salesOrderName, string line,string shift);
        IEnumerable<object> GetLineEmployeeDetail(string lineOperationBookingId);

        IEnumerable<object> GetLineCbo(string date, string plantId);

        IEnumerable<object> GetOperationCbo(string date, string linetext, string plantId);
        IEnumerable<object> GetShiftCbo(string date, string linetext, string salesorder, string plantId);

        IEnumerable<object> GetSalesOrder(string date, string lineName, string operationName, string plantId);

        void InsertLineProductionOperation(List<LineProductionOperationBookingViewModel> viewModel, DateTime toDate);

        IEnumerable<object> GetProductionBookingListByDate(string date);

        IEnumerable<object> GetForEditPrdBooking(string date, string salesOrderName, string line, string shift);
		void UpdateNoApplicablePcsRate(string id);

		void UpdateGraphLineProduction(string id, decimal prdQty, IEnumerable<LineProductionOperationBookingViewModel> entities);

        IWorkbook GetEmployeeAssignReport(string companyGroupId, string companyId, string plantName, string reportName, string date, string line);
        IWorkbook GetEmployeeReport(string companyGroupId, string companyId, string plantName, string reportName, string fromdate, string todate);
        IEnumerable<object> GetSalesOrderCbo(string date, string linetext, string plantId);

        IEnumerable<object> GetProduction(string date, string lineName, string salesOrderName, string plantId);
    }
}