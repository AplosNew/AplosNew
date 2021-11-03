using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;


namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeProfileUnApprovalController : BaseController
    {
        #region Constructor

        //private readonly IEmployeeInformationService _employeeInformationService;
        //private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        //private readonly IEmployeeResponsiblePersonService _employeeResponsiblePersonService;
        private readonly IEmployeeProfileService _employeeProfileService;//
        //private readonly IEmpReferenceInformationService _empReferenceInformationService;
        //private readonly IEmpAcademicQualificationInformationService _empAcademicQualificationInformationService;
        //private readonly IEmpExperienceInformationService _empExperienceInformationService;
        //private readonly IEmpTrainingInformationService _empTrainingInformationService;
        //private readonly IEmployeeDocumentService _employeeDocumentService;

        public EmployeeProfileUnApprovalController(
            //  IEmployeeInformationService employeeInformationService
            //, IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            //, IEmployeeResponsiblePersonService employeeResponsiblePersonService
             IEmployeeProfileService employeeProfileService
            //, IEmpReferenceInformationService empReferenceInformationService
            //, IEmpAcademicQualificationInformationService empAcademicQualificationInformationService
            //, IEmpExperienceInformationService empExperienceInformationService
            //, IEmpTrainingInformationService empTrainingInformationService
            //, IEmployeeDocumentService employeeDocumentService
            )
        {
            //_employeeInformationService = employeeInformationService;
            //_preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            //_employeeResponsiblePersonService = employeeResponsiblePersonService;
            _employeeProfileService = employeeProfileService;
            //_empReferenceInformationService = empReferenceInformationService;
            //_empAcademicQualificationInformationService = empAcademicQualificationInformationService;
            //_empExperienceInformationService = empExperienceInformationService;
            //_empTrainingInformationService = empTrainingInformationService;
            //_employeeDocumentService = employeeDocumentService;
        }

        #endregion Constructor
        EmployeeProfile employeeProfile = new EmployeeProfile();

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
      

        #endregion Pages


        #region Employee Un-Approval

        [HttpGet]
        public JsonResult GetApprovedEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_employeePromotionService.GetSalaryStrcUnApprovedEmployee(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            //JsonResult json = Json(_employeeProfileService.GetApprovedEmployeeList(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(employeeProfile.GetApprovedEmployeeList(identity.CompanyGroupId, identity.PlantId, identity.IsSysAdmin, identity.UserId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpPost]
        public ActionResult SaveUnApprovedEmployee( IEnumerable<ParaEmployeeInformation> employeeInformation)
        {

           

            DataSet dsEmployeeOTInformation = Library.Service.Helpers.DataTableExtensions.ToDataSet<ParaEmployeeInformation>(employeeInformation);
            //DataSet dsEmployeeOTInformation = null;
            _employeeProfileService.SaveUnApprovedEmployeeData(dsEmployeeOTInformation);
            return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
        }






        //[HttpGet, Authorize]
        //public JsonResult GetUnApprovalEmployeeList()
        //{

        //   // _employeeProfileService.CreateLockData(lockDate);
        //    return Json(new { Message = AplosMessage.Success });
        //}
        #endregion
    }
    
}