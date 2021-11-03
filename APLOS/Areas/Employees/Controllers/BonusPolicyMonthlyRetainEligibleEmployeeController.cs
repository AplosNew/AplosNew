#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Employees;
using Library.Service.Employees;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class BonusPolicyMonthlyRetainEligibleEmployeeController : BaseController
    {
        #region Constructor
        private readonly IBonusPolicyMonthlyRetainEligibleEmployeeService _bonusPolicyMonthlyRetainEligibleEmployeeService;
        public BonusPolicyMonthlyRetainEligibleEmployeeController(IBonusPolicyMonthlyRetainEligibleEmployeeService bonusPolicyMonthlyRetainEligibleEmployeeService)
        {
            this._bonusPolicyMonthlyRetainEligibleEmployeeService = bonusPolicyMonthlyRetainEligibleEmployeeService;
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
        public ActionResult QueryForMandatoryBonusEmployee(GridParameter parameters, string plantId)
        {
            return Json(_bonusPolicyMonthlyRetainEligibleEmployeeService.QueryForMandatoryBonusEmployee(parameters, plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult QueryForOptionalBonusEmployee(GridParameter parameters, string plantId)
        {
            return Json(_bonusPolicyMonthlyRetainEligibleEmployeeService.QueryForOptionalBonusEmployee(parameters, plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Edit(IEnumerable<BonusPolicyMonthlyRetainEligibleEmployee> bonusPolicyMonthlyRetainEligibleEmployee)
        {
            _bonusPolicyMonthlyRetainEligibleEmployeeService.InsertOrUpdate(bonusPolicyMonthlyRetainEligibleEmployee);
            return Json(new { BonusPolicyMonthlyRetainEmpWiseCalculation = bonusPolicyMonthlyRetainEligibleEmployee, Message = AplosMessage.Insert });
        }
        #endregion -- Operations
    }
}