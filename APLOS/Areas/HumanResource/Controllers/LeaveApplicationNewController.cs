using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.HumanResources;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class LeaveApplicationNewController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly ILeaveTransactionNewService _leaveTransactionService;
        private readonly IEmployeeProfileService _employeeProfileService;

        public LeaveApplicationNewController(
            ISqlRepository sqlRepository,
             ILeaveTransactionNewService leaveTransactionService,
              IEmployeeProfileService employeeProfileService
            )
        {
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;
            _employeeProfileService = employeeProfileService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult LeaveDelete()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult getLeavePolicy()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT PolicyName FROM [dbo].[LeavePolicyMaster] WHERE GroupID='" + identity.CompanyGroupId + "' and PlantID='" + identity.PlantId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(new
            {
                data
            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string yearNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.EmployeeId, yearNo), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetEmpLeaveList(GridParameter parameters, string EmpsystemId, string yearNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpsystemId, yearNo), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmpLeaveListForDelete(GridParameter parameters, string EmpsystemId, string yearNo)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.QueryGetLeaveListForDelete(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpsystemId, yearNo), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSectionEmployeeList(string sectionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(_employeeProfileService.GetSectionEmployeeList(identity.PlantId, identity.CompanyId, sectionId), JsonRequestBehavior.AllowGet);
            //JsonResult json = Json(_employeeProfileService.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            EmployeeProfile employeeProfile = new EmployeeProfile();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_employeeProfileService.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            JsonResult json = Json(employeeProfile.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadLeaveTypeCbo(identity.PlantId, identity.EmployeeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLeaveTypeCbo(string EmpsystemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadLeaveTypeCbo(identity.PlantId, EmpsystemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetYearCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadYearCbo(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLeaveBalance(string calanderYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId, identity.PlantId, identity.EmployeeId, calanderYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpLeaveBalance(string EmpsystemId, string calanderYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId, identity.PlantId, EmpsystemId, calanderYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(LeaveTransaction leaveApplication)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            leaveApplication.GroupID = identity.CompanyGroupId;
            leaveApplication.CompanyId = identity.CompanyId;
            leaveApplication.PlantID = identity.PlantId;
            leaveApplication.EmpSystemID = identity.EmployeeId;
            leaveApplication.AddedBy = identity.Name;
            leaveApplication.AppliedBy = AppliedBy.Self.ToString();
            if (string.IsNullOrEmpty(leaveApplication.AppliedDate.ToString()))
            {
                leaveApplication.AppliedDate = DateTime.Now;
            }


            _leaveTransactionService.SaveData(leaveApplication);
            return Json(new { LeaveApplication = leaveApplication, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Save(LeaveTransaction leaveApplication, string yearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            leaveApplication.GroupID = identity.CompanyGroupId;
            leaveApplication.CompanyId = identity.CompanyId;
            leaveApplication.PlantID = identity.PlantId;
            leaveApplication.AddedBy = identity.Name;
            leaveApplication.AppliedBy = AppliedBy.Self.ToString();
            leaveApplication.AppliedDate = DateTime.Now;
            _leaveTransactionService.SaveAndUpdateData(leaveApplication, yearId);
            return Json(new { LeaveApplication = leaveApplication, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(LeaveTransaction leaveApplication)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            leaveApplication.UpdatedBy = identity.Name;
            _leaveTransactionService.SaveData(leaveApplication);
            return Json(new { Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _leaveTransactionService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });

        }

        public ActionResult DeleteApprovedLeave(string id, string EmpSystemid)
        {
            _leaveTransactionService.DeleteApprovedLeaveGraph(id, EmpSystemid);
            return Json(new { Message = AplosMessage.Deleted });

        }
        [HttpGet, Authorize]
        public ActionResult LoadYearlyCalendar()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"select * from YearlyCalendar where  PlantId='" + identity.PlantId + @"' and IsYearEndClosed=0";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;



            //return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string SearchValue)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        WHERE emp.PlantID='" + identity.PlantId + @"'  and EMP.CompanyId='" + identity.CompanyId + @"' and EMP.EmployeeStatus='Active' 
                                        And emp.EmployeeCode='" + SearchValue + @"'
                                        ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}