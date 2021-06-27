#region Using

using Aplos.Properties;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Model.Payrolls;
using Library.Service.Payrolls;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.Payrolls.Controllers
{
    public class SalaryAdvanceOpeningBalanceController : BaseController
    {
        #region Constructor
        private readonly ILoanAdvanceMasterService _loanAdvanceMasterService;
        private readonly ILoanAdvanceChildService _loanAdvanceChildService;

        public SalaryAdvanceOpeningBalanceController(ILoanAdvanceMasterService loanAdvanceMasterService, ILoanAdvanceChildService loanAdvanceChildService)
        {
            _loanAdvanceMasterService = loanAdvanceMasterService;
            _loanAdvanceChildService = loanAdvanceChildService;

        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetCbo(string currencyRuleSystemID)
        {
            return Json(_loanAdvanceMasterService.GetCbo(currencyRuleSystemID).Rows, JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        public JsonResult CreateOpeningBalance(LoanAdvanceMaster loanAdvanceMaster, IEnumerable<LoanAdvanceChild> loanAdvanceChild)
        {
            _loanAdvanceMasterService.InsertOrUpdateOpeningBalance(loanAdvanceMaster, loanAdvanceChild);
            return Json(new { LoanAdvanceMaster = loanAdvanceMaster, Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetLoanMasterByEmployee(string employeeId)
        {
            return Json(_loanAdvanceMasterService.GetLoanMasterByEmployee(employeeId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOpeningBalanceByEmployee(string employeeId)
        {
            return Json(_loanAdvanceMasterService.GetOpeningBalanceByEmployee(employeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetYear(string plantId)
        {
            return Json(_loanAdvanceMasterService.GetYear(plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLoanChildByMaster(string masterId)
        {
            return Json(_loanAdvanceChildService.GetLoanChildByMaster(masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOpeningBalanceChildByMaster(string masterId)
        {
            return Json(_loanAdvanceChildService.GetOpeningBalanceChildByMaster(masterId), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}