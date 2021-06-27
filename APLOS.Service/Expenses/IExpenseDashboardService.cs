using Library.Core;
using Library.ViewModel.Accounts;
using System.Collections.Generic;

namespace Library.Service.Expenses
{
    public interface IExpenseDashboardService
    {
        IEnumerable<object> GetCompanyInformation(string companyGroupId, string companyId);

        IEnumerable<object> GetVoucherLatestDate(string compnayGroupId, string companyId, string plantId, string dateType, string itemType);

        IEnumerable<object> OrgStructureList(string companyGroupId, string companyId);

        IEnumerable<object> ExpenseList(string companyGroupId, string companyId, string factDate, string fromDate, string toDate);

        IEnumerable<object> DymnamicExpenseList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId);

        IEnumerable<object> ExpenseListLineChart(string companyGroupId, string companyId, string factDate, string fromDate, string toDate);

        IEnumerable<object> DymnamicExpenseListLineChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId);

        IEnumerable<object> DymnamicRevenueListLineChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId);

        IEnumerable<object> PeriodWiseExpenseBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId);
        IEnumerable<object> PeriodWiseRevenueBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId);

        IEnumerable<object> DynamicPeriodWiseRevenueBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId);
        IEnumerable<object> DynamicPeriodWiseExpenseBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId);


        IEnumerable<object> MonthlyExpenseVSBudgetBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId);
        IEnumerable<object> MonthlyRevenueVSBudgetBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId);

        IEnumerable<object> MonthlyDynamicExpenseVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId);
        IEnumerable<object> MonthlyDynamicRevenueVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId);

        IEnumerable<object> PeriodExpenseVSBudgetBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId);
        IEnumerable<object> PeriodRevenueVSBudgetBarChart(string factDate, string fromDate, string toDate, string companyGroupId, string companyId);

        IEnumerable<object> PeriodDynamicExpenseVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId);
        IEnumerable<object> PeriodDynamicRevenueVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId);

        GridModel ModalVoucharDetail(GridParameter parameter, string voucharNo, string budgetId, string factDate, string fromDate, string toDate, string companyGroupId, string companyId, string expenseORRevenue, string periodType);

        IEnumerable<object> ModalExpenseDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string budgetId, string factDate, string fromDate, string toDate, string companyGroupId, string companyId, string entryPeriodId, string postingPeriodId, string expenseORRevenue, string periodType);

        IEnumerable<object> ModalBudgetWiseExpense(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId, string expenseRevenue,string periodType, string postingPeriodId, string entryPeriodId);

        IEnumerable<object> RevenueListLineChart(string companyGroupId, string companyId, string factDate, string fromDate, string toDate);

        IEnumerable<object> GetFiscalYearForBarChart(string fromDate, string toDate);


    }
}