#region Using

using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Service.Expenses;
using Library.ViewModel.Accounts;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Accounts.Controllers
{
    public class ExpenseDashboardController : BaseController
    {
        private readonly IExpenseDashboardService _expenseDashBoardService;

        public ExpenseDashboardController(IExpenseDashboardService expenseDashBoardService)
        {
            _expenseDashBoardService = expenseDashBoardService;
        }
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetCompanyInformation()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.GetCompanyInformation(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetVoucherLatestDate(string dateType, string itemType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.GetVoucherLatestDate(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, dateType, itemType), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult OrgStructureList(string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.OrgStructureList(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ExpenseList(string companyGroupId, string companyId, string factDate, string fromDate, string toDate)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.ExpenseList(identity.CompanyGroupId, identity.CompanyId, factDate, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ExpenseListLineChart(string companyGroupId, string companyId, string factDate, string fromDate, string toDate)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.ExpenseListLineChart(identity.CompanyGroupId, identity.CompanyId, factDate, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult RevenueListLineChart(string companyGroupId, string companyId, string factDate, string fromDate, string toDate)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.RevenueListLineChart(identity.CompanyGroupId, identity.CompanyId, factDate, fromDate, toDate), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DymnamicExpenseList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.DymnamicExpenseList(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DymnamicExpenseListLineChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.DymnamicExpenseListLineChart(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult DymnamicRevenueListLineChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.DymnamicRevenueListLineChart(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult ModalExpenseDetail(IEnumerable<ChartColumnList> ChartColumnList, int seq, string budgetId, string factDate, string fromDate, string toDate, string companyGroupId, string companyId, string entryPeriodId, string postingPeriodId, string expenseORRevenue, string periodType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.ModalExpenseDetail(ChartColumnList, seq, budgetId, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId, entryPeriodId, postingPeriodId, expenseORRevenue, periodType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult PeriodWiseExpenseBarChart(string factDate, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.PeriodWiseExpenseBarChart(factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult PeriodWiseRevenueBarChart(string factDate, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.PeriodWiseRevenueBarChart(factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult DynamicPeriodWiseExpenseBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.DynamicPeriodWiseExpenseBarChart(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DynamicPeriodWiseRevenueBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.DynamicPeriodWiseRevenueBarChart(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult MonthlyExpenseVSBudgetBarChart(string factDate, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.MonthlyExpenseVSBudgetBarChart(factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult MonthlyRevenueVSBudgetBarChart(string factDate, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.MonthlyRevenueVSBudgetBarChart(factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult PeriodExpenseVSBudgetBarChart(string factDate, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.PeriodExpenseVSBudgetBarChart(factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult PeriodRevenueVSBudgetBarChart(string factDate, string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            return Json(_expenseDashBoardService.PeriodRevenueVSBudgetBarChart(factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult PeriodDynamicExpenseVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.PeriodDynamicExpenseVSBudgetBarChart(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult PeriodDynamicRevenueVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.PeriodDynamicRevenueVSBudgetBarChart(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult MonthlyDynamicExpenseVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.MonthlyDynamicExpenseVSBudgetBarChart(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult MonthlyDynamicRevenueVSBudgetBarChart(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.MonthlyDynamicRevenueVSBudgetBarChart(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public ActionResult ModalBudgetWiseExpense(IEnumerable<ChartColumnList> ChartColumnList, int seq, string factDate, string fromDate, string toDate, string companyGroupId, string companyId, string expenseRevenue, string periodType, string postingPeriodId, string entryPeriodId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.ModalBudgetWiseExpense(ChartColumnList, seq, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId, expenseRevenue, periodType, postingPeriodId, entryPeriodId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ModalVoucharDetail(GridParameter parameter, string voucharNo, string budgetId, string factDate, string fromDate, string toDate, string companyGroupId, string companyId, string expenseORRevenue, string periodType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.ModalVoucharDetail(parameter, voucharNo, budgetId, factDate, fromDate, toDate, identity.CompanyGroupId, identity.CompanyId, expenseORRevenue, periodType), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetFiscalYearForBarChart(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_expenseDashBoardService.GetFiscalYearForBarChart(fromDate, toDate), JsonRequestBehavior.AllowGet);
        }
    }
}