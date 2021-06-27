using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Properties;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeProbationalPeriodController : BaseController
    {
        #region Constructor

        private readonly IEmployeeProbationalPeriodService _employeeProbationalPeriodService;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IEmployeeProfileService _employeeProfileService;

        public EmployeeProbationalPeriodController(
              IEmployeeProbationalPeriodService employeeProbationalPeriodService
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
             , IEmployeeProfileService employeeProfileService)
        {
            _employeeProbationalPeriodService = employeeProbationalPeriodService;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _employeeProfileService = employeeProfileService;
        }

        #endregion Constructor

        #region -- Pages
        
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations



        [HttpGet, Authorize]
        public ActionResult GetEmployeeList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if ((!identity.IsControlAdmin && !identity.IsSysAdmin))
            {
                if (string.IsNullOrEmpty(identity.EmployeeId))
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMap));
                var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "ProbationRP", identity.EmployeeId);
                if (entity == null && !entity.Any())
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMapWithEntity));
            }
            string message = string.Empty;
            if (identity.IsSysAdmin)
                message = ServiceResources.PreRecruitmentSysAdmin;
            return Json(new
            {
                Message = message,
                Data = _employeeProbationalPeriodService.EmployeeQuery(parameters, identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyId, identity.EmployeeId, identity.PlantId)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetColorEmployeeList(GridParameter parameters, bool old, bool present, bool future)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeProbationalPeriodService.EmployeeColorQueryByDate(parameters, identity.IsControlAdmin, identity.IsSysAdmin, identity.CompanyId, identity.EmployeeId, old, present, future, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetConfirmedEmployeeData(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeProbationalPeriodService.GetConfirmedEmployeeData(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetIactiveEmployeeData(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeProbationalPeriodService.GetInActivemployeeData(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            IEnumerable<ComboModel> gcomboModels;
            gcomboModels = _employeeProbationalPeriodService.GetCbo(identity.PlantId);

            if (((List<ComboModel>)gcomboModels).Count == 0)
            {
                gcomboModels = _employeeProfileService.GetDefaultCbo(identity.CompanyGroupId, identity.PlantId);
            }

            return Json(gcomboModels, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEntity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if ((!identity.IsControlAdmin && !identity.IsSysAdmin))
            {
                if (string.IsNullOrEmpty(identity.EmployeeId))
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMap));
                var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "ProbationRP", identity.EmployeeId);
                if (entity == null || !entity.Any())
                    throw new CustomException(string.Format(ServiceResources.EmployeeNotMapWithEntity));
            }
            string message = "";
            if (identity.IsSysAdmin)
                message = ServiceResources.PreRecruitmentSysAdmin;
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProbationById(string EmployeeId)
        {
            return Json(_employeeProbationalPeriodService.ProbationQueryByID(EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEntityByEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var entity = _preRecruitmentEmployeeService.GetEntityByEmployee("HKP.ApprovalConfiguration", "ProbationRP", identity.EmployeeId);
            return Json(entity, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<EmployeeProbationalPeriod> EmployeeProbationalList)
        {
            _employeeProbationalPeriodService.ProbationalUpdate(EmployeeProbationalList);
            return Json(new { EmployeeProbationalPeriod = EmployeeProbationalList, Message = AplosMessage.Success });
        }

        public JsonResult IsConfirmed(IEnumerable<EmployeeInformation> EmployeeInformationList)
        {
            _employeeProbationalPeriodService.ConfirmedEmployeeInfo(EmployeeInformationList);
            return Json(new { EmployeeProbationalPeriod = EmployeeInformationList, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult EmployeeInActive( string EmployeeId)
        {
            
            _employeeProbationalPeriodService.UpdateStatus(EmployeeId);
            return Json(new { Message = "Status changed successfull. " });
        }
        [HttpPost]
        public JsonResult EmployeeActive(string EmployeeId)
        {

            _employeeProbationalPeriodService.UpdateStatusActive(EmployeeId);
            return Json(new { Message = "Status changed successfull. " });
        }

        [HttpPost]
        public JsonResult Edit(EmployeeProbationalPeriod model)
        {
            _employeeProbationalPeriodService.Update(model);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _employeeProbationalPeriodService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations

        #region Report

        [HttpGet, Authorize]
        public ActionResult EmployeeConfirmation(string empId, string empType, string tempId)
        {
            //string strPathHindi = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), "Con20188Hindi.xlsx"); // HttpContext.Server.MapPath("~/POPResources/Templates/CLH.xlsx");
            //string strPathEnglish = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), "Con20188English.xlsx"); // HttpContext.Server.MapPath("~/POPResources/Templates/CLE.xlsx");
            //string strPathBangla = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), "Con20188Bengali.xlsx"); // HttpContext.Server.MapPath("~/POPResources/Templates/CLB.xlsx");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "Confirmation Letter-" + empId + "" + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _employeeProbationalPeriodService.EmployeeConfirmation(identity.CompanyGroupId,identity.CompanyId, identity.PlantId, empId, empType, tempId);//, strPathHindi, strPathEnglish, strPathBangla);

            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //var fileName = "Confirmation Letter-"+ empId + "" + DateTime.Now.ToString("ddMMMyyyy") + "";
            //var workbook = _employeeProbationalPeriodService.EmployeeConfirmationLocal(identity.CompanyId, identity.PlantId, empId,"Permanent");
            ////workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            ////return View();

            return RenderReportAsExcel(workbook, fileName);

        }

        #endregion Report
    }
}