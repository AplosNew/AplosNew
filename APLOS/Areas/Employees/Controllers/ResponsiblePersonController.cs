using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.ManagementChartOfAccounts;
using Library.Service.ManagementChartOfAccounts;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class ResponsiblePersonController : BaseController
    {
        private readonly IBudgetApprovalPersonService _responsiblePersonService;
        public ResponsiblePersonController(IBudgetApprovalPersonService responsiblePersonService)
        {
            _responsiblePersonService = responsiblePersonService;
        }

        [HttpGet, Authorize]
        public JsonResult GetPotitionList(GridParameter parameters, string routineBudgetId, string activityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_responsiblePersonService.GetPotitionList(parameters, identity.CompanyGroupId, routineBudgetId, activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPotitionManpowerBudgetList(GridParameter parameters, string entityId, string routineBudgetId, string activityId)
        {
            return Json(_responsiblePersonService.GetPotitionManpowerBudgetList(parameters, entityId, routineBudgetId, activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeList(GridParameter parameters, string routineBudgetId, string activityId)
        {
            return Json(_responsiblePersonService.GetEmployeeList(parameters, routineBudgetId, activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<BudgetApprovalPerson> responsiblePersons)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _responsiblePersonService.Insert(responsiblePersons);
            return Json(new { Message = AplosMessage.Insert });
        }
    }
}