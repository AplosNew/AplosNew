using Aplos.Controllers;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Service.ManagementChartOfAccounts;
using Library.Model.ManagementChartOfAccounts;
using System.Collections.Generic;

namespace Aplos.Areas.Employees.Controllers
{
    public class DepartmentResponsiblePersonController : BaseController
    {
        private readonly IApprovalConfigurationService _approvalConfigurationService;
        private readonly IBudgetApprovalPersonService _budgetApprovalPersonService;
        public DepartmentResponsiblePersonController(
              IBudgetApprovalPersonService budgetApprovalPersonService
              , IApprovalConfigurationService ApprovalConfigurationService
            )
        {
            _approvalConfigurationService = ApprovalConfigurationService;
            _budgetApprovalPersonService = budgetApprovalPersonService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetDepartmentResponsiblePersonList(string companyId, string entityId)
        {
            return Json(_budgetApprovalPersonService.GetDepartmentResponsiblePersonList(companyId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeDataList(GridParameter parameters, string plantId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_approvalConfigurationService.GetEmployeeData(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<BudgetApprovalPerson> budgetApprovalPersons)
        {
            _budgetApprovalPersonService.InserDepartmentApprovalPerson(budgetApprovalPersons);
            return Json(new { BudgetApprovalPerson = budgetApprovalPersons, Message = AplosMessage.Success });
        }

        public ActionResult Delete(string id)
        {
            _budgetApprovalPersonService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}