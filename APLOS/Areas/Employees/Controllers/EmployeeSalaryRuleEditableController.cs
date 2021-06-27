#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.ChartOfAccounts;
using Library.Model.Employees;
using Library.Service.Accounts;
using Library.Service.Employees;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeSalaryRuleEditableController : BaseController
    {
        #region Constructor

        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly IEmployeeSalaryRuleEditableService _employeeSalaryRuleEditableService;

        public EmployeeSalaryRuleEditableController(IEmployeeSalaryRuleEditableService employeeSalaryRuleEditableService
            )
        {
            this._employeeSalaryRuleEditableService = employeeSalaryRuleEditableService;
        }

        #endregion Constructor

        #region Aplos

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Aplos

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeSalaryRuleEditableService.Query(parameters, identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<EmployeeSalaryRuleEditable> employeeSalaryRuleEditable)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeSalaryRuleEditableService.InsertUpdate(employeeSalaryRuleEditable, identity.PlantId);

            return Json(new { EmployeeSalaryRuleEditable = employeeSalaryRuleEditable, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _employeeSalaryRuleEditableService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}