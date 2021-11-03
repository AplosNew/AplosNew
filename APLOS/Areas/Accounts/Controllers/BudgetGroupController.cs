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
    public class BudgetGroupController : BaseController
    {
        private readonly IBudgetGroupService _budgetGroupService;

        public BudgetGroupController(IBudgetGroupService budgetGroupService)
        {
            _budgetGroupService = budgetGroupService;
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_budgetGroupService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_budgetGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_budgetGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BudgetGroup budgetGroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetGroupService.Insert(budgetGroup, identity.CompanyGroupId);
            return Json(new { BudgetGroup = budgetGroup, Sequence = budgetGroup.Sequence + 1, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BudgetGroup budgetGroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetGroupService.Update(budgetGroup, identity.CompanyGroupId);
            return Json(new { Sequence = _budgetGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _budgetGroupService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}