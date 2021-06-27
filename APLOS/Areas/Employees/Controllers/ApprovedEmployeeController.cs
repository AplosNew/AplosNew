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
    public class ApprovedEmployeeController : BaseController
    {
        #region Constructor

        private readonly IApprovedEmployeeService _approvedEmployeeService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;

        public ApprovedEmployeeController(
              IApprovedEmployeeService approvedEmployeeService
             , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            )
        {
            _approvedEmployeeService = approvedEmployeeService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_approvedEmployeeService.GetAllEmployee(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeData(string eId)
        {
            return Json(_approvedEmployeeService.GetEmployeeData(eId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEntityByEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "PostRecruitmentDocRP", identity.EmployeeId);
            return Json(entity, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQualificationData(string eId)
        {
            return Json(_approvedEmployeeService.GetQualificationData(eId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetExperienceData(string eId)
        {
            return Json(_approvedEmployeeService.GetExperienceData(eId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTrainingData(string eId)
        {
            return Json(_approvedEmployeeService.GetTrainingData(eId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeDocumentData(string eId)
        {
            return Json(_approvedEmployeeService.GetEmployeeDocumentData(eId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EmployeeInformation employeeInformation
                , IEnumerable<EmpAcademicQualificationInformation> empAcademicQualificationInformations
                , IEnumerable<EmpExperienceInformation> empExperienceInformations
                , IEnumerable<EmpTrainingInformation> empTrainingInformations
                , IEnumerable<EmployeeDocument> employeeDocuments
            )
        {
            _approvedEmployeeService.Insert(employeeInformation, empAcademicQualificationInformations, empExperienceInformations, empTrainingInformations, employeeDocuments);
            return Json(new { EmployeeInformation = employeeInformation, Message = AplosMessage.Success });
        }

        #endregion -- Operations
    }
}