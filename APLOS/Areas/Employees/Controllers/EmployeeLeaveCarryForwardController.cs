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
    public class EmployeeLeaveCarryForwardController : BaseController
    {
        #region Constructor

        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;

        private readonly IEmployeeLeaveSummaryService _employeeLeaveSummary;

        public EmployeeLeaveCarryForwardController(
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

        [HttpGet, Authorize]
        public JsonResult LeaveTypeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeLeaveSummary.GetLeaveTypeList(identity.CompanyGroupId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult LeaveTypeCumulativeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeLeaveSummary.GetLeaveTypeCumulativeList(identity.CompanyGroupId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetYear()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeLeaveSummary.GetYearList(identity.CompanyGroupId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetYearCboList(string plantId)
        {
            return Json(_employeeLeaveSummary.GetYearCboList(plantId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Update(EmployeeLeaveSummary employeeLeaveBalance, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeLeaveSummary.UpdateCarryForward(employeeLeaveBalance, plantId, identity.CompanyGroupId);
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