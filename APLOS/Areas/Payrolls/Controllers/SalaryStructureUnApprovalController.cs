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
using Library.Service.HumanResources;
using System;

#endregion

namespace Aplos.Areas.Payrolls.Controllers
{
    public class SalaryStructureUnApprovalController : BaseController
    {
        #region Constructor 
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly EmployeePromotionNewService _employeePromotionService;
        public SalaryStructureUnApprovalController(
            IEmployeeInformationService employeeInformationService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IEmployeeProfileService employeeProfileService
            , EmployeePromotionNewService employeePromotionService
        )
        {
            _employeeInformationService = employeeInformationService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _employeeProfileService = employeeProfileService;
            _employeePromotionService = employeePromotionService;
        }

        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult SalaryStructureUnApproval()
        {
            return View();
        }
        #endregion
        #region -- Operations
        [HttpGet]
        public JsonResult GetEmployeeListForSalaryStrcUnApproval()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_employeePromotionService.GetSalaryStrcUnApprovedEmployee(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(_employeePromotionService.GetEmployeeListForSalaryStrcUnApproval(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }









        [HttpPost]
        public JsonResult SaveSalaryStructureUnApprovalData(string EmpSystemId, string SalaryStructureId)
        {
            //CustomPara para = new CustomPara();
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //para.EmployeeId = EmpSystemId;
            //para.PlantId = identity.PlantId;
            //para.CompanyId = identity.CompanyId;
            //para.CompanyGroupId = identity.CompanyGroupId;
            //para.User = identity.Name;

            _employeePromotionService.SaveSalaryStructureUnApprovalData(EmpSystemId,SalaryStructureId);
            return Json(new {  Message = "Employee Salary Structure Unapproved Sucessfully..." }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}