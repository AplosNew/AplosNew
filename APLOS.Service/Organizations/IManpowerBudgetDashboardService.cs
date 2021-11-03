#region Using

using Library.Core;
using Library.ViewModel.Accounts;
using Library.ViewModel.Organizations;
using System.Collections.Generic;

#endregion Using

namespace Library.Service.Organizations
{
    public interface IManpowerBudgetDashboardService
    {
        IEnumerable<object> GroupWiseCompanyList(string date, string status, string EmplyeeTypeOrCategoryId);

        //IEnumerable<object> DrillDownList(string CompanyGroupId);
        IEnumerable<object>  DrillDownList(string CompanyGroupId, string CompanyId);

        IEnumerable<object> CompanyWiseDrillDownList(string companyGroupId, string companyId);

        IEnumerable<object> DetailDrillDownTable(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string state, string EmplyeeTypeOrCategoryId);

        IEnumerable<OrgStructureListViewModel> OrgStructureList(string CompanyGroupId, string CompanyId);

        IEnumerable<object> ModalGroupWiseEmlpoyeeList(string CompanyGroupId, IEnumerable<ChartColumnList> ChartColumnList, int seq, string status, string EmplyeeTypeOrCategoryId);

        IEnumerable<object> ModalEmlpoyeeListDetail(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, string companyId, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        IEnumerable<object> ModalBudgetSummary(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        IEnumerable<object> ModalBudgetDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        IEnumerable<object> ModalExcessSummary(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, int seq, string date, string status, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        IEnumerable<object> ModalExcessDetail(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, int seq, string date, string status, GridParameter parameters);

        IEnumerable<object> ModalShortSummary(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, int seq, string date, string status, GridParameter parameters);

        IEnumerable<object> ModalShortDetail(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, int seq, string date, string status, GridParameter parameters);

        IEnumerable<object> BudgetCodeWiseEmpList(IEnumerable<ChartColumnList> ChartColumnList, string companyGroupId, string budgetCode, string EmplyeeTypeOrCategoryId, GridParameter parameters);

        IEnumerable<object> WpBudgetCodeWiseEmpList(IEnumerable<ChartColumnList> ChartColumnList, string budgetCode);
    }
}