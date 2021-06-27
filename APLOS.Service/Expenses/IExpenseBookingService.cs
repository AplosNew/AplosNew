using Library.Core;
using Library.Model.Enums;
using Library.Model.Expenses;
using Library.Service.Core;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using System.Collections.Generic;
using System.Data;

namespace Library.Service.Expenses
{
    public interface IExpenseBookingService : IService<ExpenseBooking>
    {
        void Insert(ExpenseBooking entity, IEnumerable<ExpenseBookingDetail> details, IEnumerable<ExpenseActivity> expActdetails);

        void Update(ExpenseBooking entity, IEnumerable<ExpenseBookingDetail> details, IEnumerable<ExpenseActivity> expActdetails);

        void EntityExpenseBookingSubmit(ExpenseBooking entity, IEnumerable<ExpenseBookingDetail> details);

        GridModel Query(GridParameter parameters, string expenseBookingId);

        GridModel GetExpenseBookingApprovedData(GridParameter parameters, string companyId, string plantId, string expensesBookingId);

        GridModel GetEntityExpenseBookingSubmittedData(GridParameter parameters, string companyId, string plantId, string expensesBookingId);

        IEnumerable<object> GetExpenseBookingDetail(string expenseBookingId);

        IEnumerable<ExpenseBookingViewModel> GetExpenseBookingPendingList(string employeeId);

        GridModel GetExpenseBookingPendingList(GridParameter parameters, string companyGroupId, string companyId, string plantId);

        GridModel GetExpenseBookingApprovedList(GridParameter parameters, string companyGroupId, string companyId, string plantId);

        GridModel GetEntityExpenseBookingPendingList(GridParameter parameters, string companyGroupId, string companyId, string plantId);

        GridModel GetEntityExpenseBookingSubmittedList(GridParameter parameters, string companyGroupId, string companyId, string plantId);

        void InsertExpenseBookingApproved(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);

        GridModel QueryExpenseBookingApproved(GridParameter parameters, string companyGroupId, string companyId);

        GridModel QueryPoatal(GridParameter parameters, string status);
        IEnumerable<object> QueryPoatal(string status);
        IEnumerable<object> QueryCheckedByPoatal(string status);
        GridModel QueryAdmin(GridParameter parameters, string status);

        GridModel GetEntityExpenseBooking(GridParameter parameters, string status);

        GridModel GetListForApproval(GridParameter parameters);

        IEnumerable<object> GetListForDepartmentApproval(string approvalStatus);
        IEnumerable<object> GetListForDepartmentApprovedHoldReject();
        void ExpenseBookingApprovalPotal(ExpenseBooking entity, IEnumerable<ExpenseBookingDetail> details, string responsiblePersonId);
        void ExpenseBookingCheckedPotal(ExpenseBooking entity, IEnumerable<ExpenseBookingDetail> details, string responsiblePersonId);
        void ApprovalGraph(IEnumerable<ExpenseBookingDetail> entities, ExpenseBooking expenseBooking, string responsiblePersonId);
        void CheckedGraph(IEnumerable<ExpenseBookingDetail> expenseBookingDetails, ExpenseBooking expenseBooking, string responsiblePersonId);
        string GetEmployeeTransactionNo(string employeeId);

        Dictionary<string, object> GetExpenseBookingReportHeader(string companyGroupId, string companyId, string plantId, string expensesBookingId);

        List<Dictionary<string, object>> GetExpenseBookingReportData(string expensesBookingId);

        DataTable GetEmployeeExpenseBookingData(string companyGroupId, string companyId, string plantId, string employeeId, string fromDate, string toDate);

        DataTable GetAssetRegisterExpenseBookingData(string companyGroupId, string companyId, string plantId, string fixedAssetRegisterId, string fromDate, string toDate);

        string InsertEntityExpenses(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        List<Dictionary<string, object>> GetEntityExpensesBookingDetail(string companyGroupId, string companyId, string plantId, string voucherId, SourceType sourceType);
        string UpdateCashJournal(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> voucherDetailVMList);
        Dictionary<string, object> GetExpenseBookingFile(string id);
        IEnumerable<object> CheckedQueryByCheckedBy();
        void DeleteApprovedExpenseBooking(string expensesBookingId);
    }
}