#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class ResignationRecruitmentPlanningController : BaseController
	{
		#region Constructor
		private readonly IResignationReqcruitmentPlanningService _ResignationReqcruitmentPlanningService;
	    private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IResignationReqcruitmentPlanningService _resignationReqcruitmentPlanningService;

        public ResignationRecruitmentPlanningController(
              IResignationReqcruitmentPlanningService ResignationReqcruitmentPlanningService
              ,IPreRecruitmentEmployeeService preRecruitmentEmployeeService
              , ResignationReqcruitmentPlanningService resignationReqcruitmentPlanningService
            )
		{
            _ResignationReqcruitmentPlanningService = ResignationReqcruitmentPlanningService;
		    _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _resignationReqcruitmentPlanningService = resignationReqcruitmentPlanningService;
        }
		#endregion

		#region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetEntity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if ((!identity.IsControlAdmin && !identity.IsSysAdmin))
            {
                if (string.IsNullOrEmpty(identity.EmployeeId))
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMap));
                var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "ResigRecruitPlanningRP", identity.EmployeeId);
                if (entity == null || !entity.Any())
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMapWithEntity));
            }
            string message = null;
            if (identity.IsSysAdmin)
                message = ServiceResources.PreRecruitmentSysAdmin;
            return Json(message, JsonRequestBehavior.AllowGet);
        }
        

        [HttpGet, Authorize]
        public ActionResult GetListRecPlanning(GridParameter parameters, string companyId, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ResignationReqcruitmentPlanningService.ResignedEmployeeQuery(parameters, companyId, plantId, identity.IsControlAdmin, identity.IsSysAdmin, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListRecPlanningByEmpId(GridParameter parameters, string companyId, string plantId, string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_ResignationReqcruitmentPlanningService.ResignedEmployeeQueryByEmpId(parameters, companyId, plantId, empId, identity.IsControlAdmin, identity.IsSysAdmin, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(IEnumerable<RecruitmentPlanningProcessSet> RecruitmentPlanningProcessSet)
        {
            _resignationReqcruitmentPlanningService.ProcessSetInsert(RecruitmentPlanningProcessSet);
            return Json(new { EmployeeProbationalPeriod = RecruitmentPlanningProcessSet, Message = AplosMessage.Success });
        }


        [HttpGet, Authorize]
        public ActionResult GetEntityByEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "ResigRecruitPlanningRP", identity.EmployeeId);
            return Json(entity, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}