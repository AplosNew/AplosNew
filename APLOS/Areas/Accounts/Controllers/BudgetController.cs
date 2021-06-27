using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.ManagementChartOfAccounts;
using Library.Service.ManagementChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class BudgetController : BaseController
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_budgetService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_budgetService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_budgetService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Budget budget)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetService.Insert(budget, identity.CompanyGroupId);
            return Json(new { Budget = budget, Sequence = _budgetService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Budget budget)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetService.Update(budget, identity.CompanyGroupId);
            return Json(new { Sequence = _budgetService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _budgetService.DeleteBudget(id);
            return Json(new { Sequence = _budgetService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}