#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Payrolls;
using Library.Model.Setups;
using Library.Service.Employees;
using Library.Service.HumanResources;
using Library.Service.Properties;
using Library.Service.Setups;
using Library.ViewModel.HR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class SalaryFixationController : BaseController
    {
        #region -- Constructor

        private readonly ISalaryFixationService _salaryFixationService;
        private readonly ISalaryFixationMailService _sfm;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;

        public SalaryFixationController(ISalaryFixationService salaryFixationService, IPreRecruitmentEmployeeService preRecruitmentEmployeeService, ISalaryFixationMailService sfm)
        {
            _salaryFixationService = salaryFixationService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _sfm = sfm;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize]
        public ActionResult AplosMail()
        {
            return View();
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salaryFixationService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetEmployees(GridParameter parameters)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if ((!identity.IsControlAdmin && !identity.IsSysAdmin))
            {
                if (string.IsNullOrEmpty(identity.EmployeeId))
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMap));
                var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "SalaryFixationApproval", identity.EmployeeId);
                if (entity == null || !entity.Any())
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMapWithEntity));
            }
            string message = "";
            if (identity.IsSysAdmin)
                message = ServiceResources.PreRecruitmentSysAdmin.ToString();
            return Json
                (new
                {
                    Message = message,
                    Data = _salaryFixationService.GetEmployees(parameters, identity.CompanyGroupId, identity.CompanyId),
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadDetails(string preRecEmpId)
        {
            return Json(_salaryFixationService.GetSalaryHeadList(preRecEmpId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTermsAndConditionsByEmployee(string preRecruitmentEmployeeid)
        {
            return Json(_salaryFixationService.GetTermsAndConditionsByEmployee(preRecruitmentEmployeeid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GettermsAndConditionsByPlant(string plantId)
        {
            return Json(_salaryFixationService.GetTermsAndConditionsByPlant(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSalaryHeadDataList()
        {
            return Json(_salaryFixationService.GetSalaryHeadDataList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetGDAndEmpWiseSalaryHeadList(string preRecruitmentEmployeeId, string givenDesignationId)
        {
            return Json(_salaryFixationService.GetGDAndEmpWiseSalaryHeadList(preRecruitmentEmployeeId, givenDesignationId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetHeadList(string preRecruitmentEmployeeId, string givenDesignationId, string plantId)
        {
            return Json(_salaryFixationService.GetHeadList(preRecruitmentEmployeeId, givenDesignationId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCalculationInfo(string preRecruitmentEmployeeId, string givenDesignationId, string plantId)
        {
            return Json(_salaryFixationService.GetCalculationInfo(preRecruitmentEmployeeId, givenDesignationId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateSalary(IEnumerable<SalaryFixation> salaryFixationList, string companyGroupId, string plantid)
        {
            _salaryFixationService.InsertOrUpdateGraph(salaryFixationList, companyGroupId, plantid);
            return Json(new { SalaryFixation = salaryFixationList, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<SalaryFixation> salaryFixationList, string plantid, EmployeeWiseTermsAndConditions employeeWiseTermsAndConditions, bool ismail)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _salaryFixationService.InsertOrUpdateGraphFromFixation(salaryFixationList, identity.CompanyGroupId, plantid, employeeWiseTermsAndConditions, ismail);
            return Json(new { SalaryFixation = salaryFixationList, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult SendMail(string empid, string plantid)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _sfm.InsertOrUpdateSFMail(empid, plantid);
            return Json(new { Message = "Mail sent request submitted" });
        }

        [HttpPost]
        public JsonResult Calculate(IEnumerable<SalaryFixation> salaryFixationList, string totalsalary, string empid, string designationid, string plantId)
        {
            IEnumerable<SalaryFixationVM> list = null;
            _salaryFixationService.GetCalculationInfoFinal(salaryFixationList, totalsalary, empid, designationid, plantId, out list);
            return Json(new { SalaryFixation = list, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Edit(SalaryFixation model)
        {
            _salaryFixationService.Update(model);
            return Json(new { SalaryFixation = model, Message = AplosMessage.Updated });
        }

        #endregion -- Operations
    }
}