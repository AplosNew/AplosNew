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
    public class BudgetClassController : BaseController
    {
        private readonly IBudgetClassService _budgetClassService;

        public BudgetClassController(IBudgetClassService budgetClassService)
        {
            _budgetClassService = budgetClassService;
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_budgetClassService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_budgetClassService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_budgetClassService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BudgetClass budgetClass)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetClassService.Insert(budgetClass, identity.CompanyGroupId);
            return Json(new { BudgetClass = budgetClass, Sequence = budgetClass.Sequence + 1, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BudgetClass budgetClass)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _budgetClassService.Update(budgetClass, identity.CompanyGroupId);
            return Json(new { Sequence = _budgetClassService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _budgetClassService.Delete(id);
            return Json(new { Message = AplosMessage.Success });
        }
    }
}