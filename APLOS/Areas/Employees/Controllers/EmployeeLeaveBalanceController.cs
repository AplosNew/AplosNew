#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Properties;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeLeaveBalanceController : BaseController
    {
        #region Constructor

        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeLeaveSummaryService _employeeLeaveSummary;

        public EmployeeLeaveBalanceController(
              IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IEmployeeLeaveSummaryService employeeLeaveSummary
            )
        {
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _employeeLeaveSummary = employeeLeaveSummary;
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
        public ActionResult ActiveEmployeeList(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeLeaveSummary.ActiveEmpListByPlantId(parameters, plantId, identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyGroupId, identity.CompanyId, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Update(EmployeeLeaveSummary employeeLeaveBalance, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeLeaveSummary.UpdateLeaveBalance(employeeLeaveBalance, plantId, identity.CompanyGroupId);
            return Json(new { EmployeeLeaveSummary = employeeLeaveBalance, Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetEntity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if ((!identity.IsControlAdmin && !identity.IsSysAdmin))
            {
                if (string.IsNullOrEmpty(identity.EmployeeId))
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMap));
                var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "ResignationApply", identity.EmployeeId);
                if (entity == null || !entity.Any())
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMapWithEntity));
            }
            string message = null;
            if (identity.IsSysAdmin)
                message = ServiceResources.PreRecruitmentSysAdmin;
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LeaveSummary(string CompanyGroupId)
        {
            _employeeLeaveSummary.Save(CompanyGroupId);
            return Json(new { Message = AplosMessage.Updated });
        }

        #endregion
    }
}