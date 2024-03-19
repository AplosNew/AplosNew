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
    public class RecruitmentApprovalController : BaseController
	{
		#region Constructor
		private readonly IPreRecruitmentApprovalService _preRecruitmentEmployee;
		private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
		public RecruitmentApprovalController(
			  IPreRecruitmentApprovalService preRecruitmentEmployee
			 , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
			)
		{
			_preRecruitmentEmployee = preRecruitmentEmployee;
			_preRecruitmentEmployeeService = preRecruitmentEmployeeService;

		}
		#endregion

		#region -- Pages

		public ActionResult Aplos()
		{
			return View();
		}
		#endregion

		#region -- Operations

		[HttpGet]
		public ActionResult GetList(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			if ((!identity.IsControlAdmin && !identity.IsSysAdmin))
			{
				if (string.IsNullOrEmpty(identity.EmployeeId))
					throw new CustomException(string.Format(ServiceResources.EmployeeNotMap));
				var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "RecruitmentFinalConfirmationRP", identity.EmployeeId);
				if (entity == null || !entity.Any())
					throw new CustomException(string.Format(ServiceResources.EmployeeNotMapWithEntity));
			}
			var message = "";
			if (identity.IsSysAdmin)
				message = ServiceResources.PreRecruitmentSysAdmin;
			return Json(new
			{
				Message = message,
				Data = _preRecruitmentEmployee.GetData(parameters, identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyGroupId, identity.CompanyId, identity.EmployeeId)
			}, JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public ActionResult GetEntityByEmployee()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "RecruitmentFinalConfirmationRP", identity.EmployeeId);
			return Json(entity, JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public JsonResult GetGivenDesignationCbo()
		{
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployee.GetGivenDesignationCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public JsonResult GetLegalDesignationCbo(GridParameter parameters,string companyGroupId, string BudgetCode)
		{
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           // return Json(_bloodGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
            return Json(_preRecruitmentEmployee.GetLegalDesignationCbo(parameters,companyGroupId, identity.PlantId, BudgetCode), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public JsonResult GetDesignationCbo(GridParameter parameters, string companyGroupId, string BudgetCode)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_preRecruitmentEmployee.GetDesignationCbo(parameters, companyGroupId, identity.PlantId, BudgetCode), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
        public JsonResult GetLegalDesignationCbobyGivenDesignation(string givenDesignationpId)
        {
            return Json(_preRecruitmentEmployee.GetLegalDesignationCbobyGivenDesignation(givenDesignationpId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
		public JsonResult Create(IEnumerable<PreRecruitmentEmployee> preRecruitmentEmployees)
		{
			_preRecruitmentEmployee.InsertORUpdate(preRecruitmentEmployees);
			return Json(new { Message = AplosMessage.Insert });
		}
		#endregion
	}
}