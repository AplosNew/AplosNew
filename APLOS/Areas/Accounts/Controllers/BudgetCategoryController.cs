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
    public class BudgetCategoryController : BaseController
    {
        private readonly IBudgetCategoryService _budgetCategoryService;

        public BudgetCategoryController(IBudgetCategoryService budgetCategoryService)
        {
            _budgetCategoryService = budgetCategoryService;
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_budgetCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_budgetCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_budgetCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BudgetCategory budgetCategory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetCategoryService.Insert(budgetCategory, identity.CompanyGroupId);
            return Json(new { BudgetCategory = budgetCategory, Sequence = budgetCategory.Sequence + 1, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BudgetCategory budgetCategory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetCategoryService.Update(budgetCategory, identity.CompanyGroupId);
            return Json(new { Sequence = _budgetCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _budgetCategoryService.DeleteBudgetCategory(id);
            return Json(new { Message = AplosMessage.Success });
        }
    }
}