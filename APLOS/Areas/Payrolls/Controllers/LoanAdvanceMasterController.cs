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
	public class LoanAdvanceMasterController : BaseController
	{
		#region Constructor
		private readonly ILoanAdvanceMasterService _loanAdvanceMasterService;
		private readonly ILoanAdvanceChildService _loanAdvanceChildService;

		public LoanAdvanceMasterController(ILoanAdvanceMasterService loanAdvanceMasterService, ILoanAdvanceChildService loanAdvanceChildService)
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

        [HttpGet, Authorize]
        public JsonResult GetSalaryHeadCbo(string currencyRuleSystemID)
        {
            return Json(_loanAdvanceMasterService.GetSalaryHeadCbo(currencyRuleSystemID), JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
		public JsonResult Create(LoanAdvanceMaster loanAdvanceMaster, IEnumerable<LoanAdvanceChild> loanAdvanceChild)
		{
			_loanAdvanceMasterService.InsertOrUpdate(loanAdvanceMaster, loanAdvanceChild);
			return Json(new { LoanAdvanceMaster = loanAdvanceMaster, Message = AplosMessage.Success });
		}

		[HttpGet, Authorize]
		public ActionResult GetLoanMasterByEmployee(string employeeId)
		{
			return Json(_loanAdvanceMasterService.GetLoanMasterByEmployee(employeeId), JsonRequestBehavior.AllowGet);
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
        #endregion
    }
}