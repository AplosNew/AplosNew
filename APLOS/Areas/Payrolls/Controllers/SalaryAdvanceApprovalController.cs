#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Payrolls;
using Library.Service.Payrolls;
using System.Collections.Generic;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Service.Employees;
using Library.Service.Properties;
using System.Linq;
using System.Web.Mvc;
using Library.Data;

#endregion

namespace Aplos.Areas.Payrolls.Controllers
{
    public class SalaryAdvanceApprovalController : BaseController
    {
        #region Constructor
        private readonly ILoanAdvanceMasterService _loanAdvanceMasterService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        public SalaryAdvanceApprovalController(ILoanAdvanceMasterService loanAdvanceMasterService, IPreRecruitmentEmployeeService preRecruitmentEmployeeService)
        {
            _loanAdvanceMasterService = loanAdvanceMasterService;
            _preRecruitmentEmployeeService= preRecruitmentEmployeeService;
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
        [HttpPost]
        public JsonResult Create(LoanAdvanceMaster loanAdvanceMaster, IEnumerable<LoanAdvanceChild> loanAdvanceChild)
        {
            _loanAdvanceMasterService.InsertOrUpdate(loanAdvanceMaster, loanAdvanceChild);
            return Json(new { LoanAdvanceMaster = loanAdvanceMaster, Message = AplosMessage.Success });
        }
        [HttpPost]
        public JsonResult UpdateSalaryApprovalDetails(IEnumerable<LoanAdvanceMaster> models)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _loanAdvanceMasterService.UpdateSalApprovals(models,identity.Name);
            return Json(new { LoanAdvanceMaster = models, Message = AplosMessage.Success });
        }
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if ((!identity.IsControlAdmin && !identity.IsSysAdmin))
            {
                if (string.IsNullOrEmpty(identity.EmployeeId))
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMap));
                var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "SalaryAdvanceApproval", identity.EmployeeId);
                if (entity == null || !entity.Any())
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMapWithEntity));
            }
            string message = "";
            if (identity.IsSysAdmin)
                message = ServiceResources.PreRecruitmentSysAdmin.ToString();
            return Json(new
            {
                Message = message,
                Data = _loanAdvanceMasterService.GetLoanAdvanceInfoPlantWise(parameters, plantId,identity.IsControlAdmin, identity.IsSysAdmin,identity.EmployeeId)
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}