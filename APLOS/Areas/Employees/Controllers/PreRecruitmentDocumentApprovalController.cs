#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Service.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class PreRecruitmentDocumentApprovalController : BaseController
    {
        #region Constructor
        private readonly IPreRecruitmentDocumentApprovalService _PreRecruitmentDocumentApprovalService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;

        public PreRecruitmentDocumentApprovalController(
              IPreRecruitmentDocumentApprovalService PreRecruitmentDocumentApprovalService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            )
        {
            _PreRecruitmentDocumentApprovalService = PreRecruitmentDocumentApprovalService;
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

        [HttpGet, Authorize]
        public JsonResult GetEmployeeData(string eId)
        {
            return Json(_PreRecruitmentDocumentApprovalService.GetEmployeeData(eId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeDocumentData(string eId)
        {
            return Json(_PreRecruitmentDocumentApprovalService.GetEmployeeDocumentData(eId), JsonRequestBehavior.AllowGet);
        }

		[HttpGet, Authorize]
		public ActionResult GetList(GridParameter parameters)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			if ((!identity.IsControlAdmin && !identity.IsSysAdmin))
			{
				if (string.IsNullOrEmpty(identity.EmployeeId))
					throw new CustomException(string.Format(ServiceResources.EmployeeNotMap));
				var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "PreRecruitmentDocRP", identity.EmployeeId);
				if (entity == null || !entity.Any())
					throw new CustomException(string.Format(ServiceResources.EmployeeNotMapWithEntity)); 
			}
			string message = "";
			if (identity.IsSysAdmin)
				message = ServiceResources.PreRecruitmentSysAdmin;
			return Json(new
			{
				Message = message,
				Data = _PreRecruitmentDocumentApprovalService.GetAllSubmittedEmployee(parameters, identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyGroupId, identity.CompanyId, identity.EmployeeId)
			}, JsonRequestBehavior.AllowGet);
		}

        [HttpGet, Authorize]
        public ActionResult GetEntityByEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "PreRecruitmentDocRP", identity.EmployeeId);
            return Json(entity, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PreRecruitmentEmployee preRecruitmentEmployees
                ,IEnumerable<PreRecruitmentEmpQualification> preRecruitmentEmpQualification
                ,IEnumerable<PreRecruitmentEmpExperience> preRecruitmentEmpExperience
                ,IEnumerable<PreRecruitmentEmpTraining> preRecruitmentEmpTraining
                ,IEnumerable<PreRecruitmentDocument> preRecruitmentDocument
            )
        {
            _PreRecruitmentDocumentApprovalService.Insert(preRecruitmentEmployees, preRecruitmentEmpQualification, preRecruitmentEmpExperience, preRecruitmentEmpTraining, preRecruitmentDocument);
            return Json(new { PreRecruitmentEmployee = preRecruitmentEmployees, Message = AplosMessage.Success});
        }
        #endregion
    }
}