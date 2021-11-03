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
    public class BudgetSubCategoryController : BaseController
    {
        private readonly IBudgetSubCategoryService _budgetSubCategoryService;

        public BudgetSubCategoryController(IBudgetSubCategoryService budgetSubCategoryService)
        {
            _budgetSubCategoryService = budgetSubCategoryService;
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_budgetSubCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_budgetSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_budgetSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BudgetSubCategory budgetSubCategory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetSubCategoryService.Insert(budgetSubCategory, identity.CompanyGroupId);
            return Json(new { BudgetSubCategory = budgetSubCategory, Sequence = budgetSubCategory.Sequence + 1, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BudgetSubCategory budgetSubCategory)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetSubCategoryService.Update(budgetSubCategory, identity.CompanyGroupId);
            return Json(new { Sequence = _budgetSubCategoryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _budgetSubCategoryService.DeleteBudgetSubCategory(id);
            return Json(new { Message = AplosMessage.Success });
        }
    }
}