#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Employees;
using Library.Service.Employees;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class ApprovalConfigurationController : BaseController
    {
        #region Constructor

        private readonly IApprovalConfigurationService _ApprovalConfigurationService;

        public ApprovalConfigurationController(
              IApprovalConfigurationService ApprovalConfigurationService
            )
        {
            _ApprovalConfigurationService = ApprovalConfigurationService;
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
        public ActionResult GetList(GridParameter parameters, string plantId, string entityId)
        {
            return Json(_ApprovalConfigurationService.Query(parameters, plantId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeDataList(GridParameter parameters, string plantId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeData(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetAllEmployeeData(GridParameter parameters)
        {
            return Json(_ApprovalConfigurationService.GetAllEmployeeData(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetEmployeeWithoutPayrollGroupData(string plantId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeWithoutPayrollGroupData(plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeWithoutAttendanceGroupData(string plantId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeWithoutAttendanceGroupData(plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeWithSalaryProcessData(GridParameter parameters, string plantId,string MonthId,string YearId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeWithSalaryProcessData(parameters, plantId, MonthId, YearId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeWithoutPaidhoursData(GridParameter parameters, string plantId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeWithoutPaidhoursData(parameters, plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeDataWithEmployeeCode(string employeeCode)
        {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new { results = _ApprovalConfigurationService.GetEmployeeDataWithEmployeeCode(identity.PlantId, employeeCode) }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeDataByCompany(GridParameter parameters, string companyId)
        {
            return Json(_ApprovalConfigurationService.GetEmployeeDataByCompany(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeDataWithIds(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeDataWithIds(parameters, plantId, departmentIds, divisionIds, sectionIds,employeeCateogoryIds,givenDesignationIds, employeeCode, employeeName), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeDataIds(GridParameter parameters, string plantId, string lineIds, string employeeCode, string employeeName, string SubsectionId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeDataIds(parameters, plantId, lineIds, employeeCode, employeeName, SubsectionId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeAttendanceGroupDataWithIds(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeAttendanceGroupDataWithIds(parameters, plantId, departmentIds, divisionIds, sectionIds, employeeCateogoryIds, givenDesignationIds, employeeCode, employeeName), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeAttendanceGroupDataWithLine(GridParameter parameters, string plantId, string lineIds, string employeeCode, string employeeName, string SubsectionId)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeAttendanceGroupDataWithLine(parameters, plantId, lineIds, employeeCode, employeeName, SubsectionId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeDataWithPaidHoursIds(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeDataWithPaidHoursIds(parameters, plantId, departmentIds, divisionIds, sectionIds, employeeCateogoryIds, givenDesignationIds, employeeCode, employeeName), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmployeeDataWithfilter(GridParameter parameters, string plantId, string departmentIds, string divisionIds, string sectionIds, string employeeCateogoryIds, string givenDesignationIds, string employeeCode, string employeeName)
        {
            if (string.IsNullOrEmpty(plantId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                plantId = identity.PlantId;
            }
            return Json(_ApprovalConfigurationService.GetEmployeeDataWithfilters(parameters, plantId, departmentIds, divisionIds, sectionIds, employeeCateogoryIds, givenDesignationIds, employeeCode, employeeName), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(ApprovalConfiguration model)
        {
            _ApprovalConfigurationService.Insert(model);
            return Json(new { ApprovalConfiguration = model, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ApprovalConfiguration model)
        {
            _ApprovalConfigurationService.Update(model);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _ApprovalConfigurationService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}