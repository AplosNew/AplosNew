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
    public class LeaveOpeningBalanceController : BaseController
    {
        #region Constructor

        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly ILeaveOpeningBalanceService _leaveOpeningBalanceService;

        public LeaveOpeningBalanceController(ILeaveOpeningBalanceService leaveOpeningBalanceService
            )
        {
            this._leaveOpeningBalanceService = leaveOpeningBalanceService;
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
            return Json(_leaveOpeningBalanceService.Query(parameters, identity.PlantId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLeaveTypeList(GridParameter parameters, string employeeId, string calendarId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveOpeningBalanceService.GetLeaveTypeList(parameters, employeeId, calendarId, identity.PlantId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<LeaveOpeningBalance> leaveOpeningBalance)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _leaveOpeningBalanceService.InsertUpdate(leaveOpeningBalance, identity.PlantId);

            return Json(new { LeaveOpeningBalance = leaveOpeningBalance, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _leaveOpeningBalanceService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}