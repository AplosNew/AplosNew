using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Setups;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeMyAppLeaveApplicationController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly ILeaveTransectionService _leaveTransactionService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly IMailSenderService _mailSenderService;
        private readonly ILeaveTransactionNewService _leaveTransactionNewService;

        public EmployeeMyAppLeaveApplicationController(
            ISqlRepository sqlRepository,
             ILeaveTransectionService leaveTransactionService,
              IEmployeeProfileService employeeProfileService
             , IMailSenderService mailSenderService
            , ILeaveTransactionNewService leaveTransactionNewService
            )
        {
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;
            _leaveTransactionNewService = leaveTransactionNewService;
            _employeeProfileService = employeeProfileService;
            _mailSenderService = mailSenderService;
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
        public ActionResult GetResponsiblePersonBudgetCode()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //return Json(_employeeProfileService.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            DataTable dtEMPBudgetCode = _sqlRepository.GetDataTable(@"SELECT * FROM EmployeeInformation EEI  where SystemId = '" + identity.EmployeeId + @"'");
            DataTable dtRPBudgetCode = _sqlRepository.GetDataTable(@"SELECT * FROM MST.ManpowerBudget WHERE Id = '" + dtEMPBudgetCode.Rows[0]["BudgetCode"].ToString() + @"' ");

            //string strSql = @"SELECT * FROM EmployeeInformation EEI 
            //        --LEFT JOIN MST.ManpowerBudget MB ON EEI.BudgetCode = MB.ROBudgetCode
            //        WHERE  EEI.EmployeeStatus = 'Active' AND EEI.BudgetCode = (select Id from MST.ManpowerBudget where Code ='" + dtRPBudgetCode.Rows[0]["ROBudgetCode"].ToString() + @"')   AND
            //        EEI.DOJ = (SELECT MIN(DOJ) FROM EmployeeInformation WHERE BudgetCode = (select Id from MST.ManpowerBudget where Code ='" + dtRPBudgetCode.Rows[0]["ROBudgetCode"].ToString() + @"'))";
            string strSql = @"SELECT TOP 1 * FROM EmployeeInformation WHERE BudgetCode = '"+ dtRPBudgetCode.Rows[0]["ROBudgetCode"].ToString() + "' AND EmployeeStatus = 'Active'  ORDER BY DOJ DESC";
            var dtRPEEI = _sqlRepository.GetDataCollection(strSql);

            JsonResult json = Json(dtRPEEI, JsonRequestBehavior.AllowGet);
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

            return Json(_leaveTransactionNewService.LoadGrdAllocatedLvDetailsNew(identity.CompanyGroupId, identity.PlantId, identity.EmployeeId, calanderYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpLeaveBalance(string EmpsystemId, string calanderYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId, identity.PlantId, EmpsystemId, calanderYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(LeaveTransaction leaveApplication, string responsiblePersonId, string responsiblePersonName, string responsiplePersonEmail,string yearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            leaveApplication.GroupID = identity.CompanyGroupId;
            leaveApplication.CompanyId = identity.CompanyId;
            leaveApplication.PlantID = identity.PlantId;
            leaveApplication.EmpSystemID = identity.EmployeeId;
            leaveApplication.AddedBy = identity.FullName;
            leaveApplication.AppliedBy = AppliedBy.Self.ToString();
            leaveApplication.FirstApprovingAuthority = responsiblePersonId;
            if (string.IsNullOrEmpty(leaveApplication.AppliedDate.ToString()))
            {
                leaveApplication.AppliedDate = DateTime.Now;
            }


            _leaveTransactionService.SaveData(leaveApplication, yearId);

            #region Email Sender Service

            if(!string.IsNullOrEmpty(responsiblePersonId))
            {
                string mailMessage = "";
                DataTable dtEmpInfo = _sqlRepository.GetDataTable(@"select * from EmployeeInformation where SystemId = '" + leaveApplication.EmpSystemID + @"'");

                var dtFmDate = Convert.ToDateTime(leaveApplication.FromDate);
                var dtToDate = Convert.ToDateTime(leaveApplication.ToDate);

                TimeSpan difference = dtToDate - dtFmDate;
                var leaveDays = Convert.ToInt32(difference.Days + 1);
                leaveApplication.LeaveStatus = LeaveStatus.Pending.ToString();


                if (leaveApplication.LeaveDayType == "FirstHalfDay" || leaveApplication.LeaveDayType == "SecondHalfDay")
                {
                    leaveApplication.LeaveDays = 0.5m;
                    if (leaveApplication.LeaveDayType == "FirstHalfDay")
                    {
                        mailMessage = @"Dear " + responsiblePersonName + "<br> <br> <br>" +
                                        " You have a Leave Approval request of " + dtEmpInfo.Rows[0]["EmployeeName"].ToString() + "(" + dtEmpInfo.Rows[0]["EmployeeCode"].ToString() + ") For First Half  Dated On" + leaveApplication.FromDate.ToString("dd-MMM-yyyy") +
                                        ". Please go to the portal for Approving." +
                                        "<br> <br> <br>" +
                                        "Thank you";
                    }
                    if (leaveApplication.LeaveDayType == "SecondHalfDay")
                    {
                        mailMessage = @"Dear " + responsiblePersonName + "<br> <br> <br>" +
                                        " You have a Leave Approval request of " + dtEmpInfo.Rows[0]["EmployeeName"].ToString() + "(" + dtEmpInfo.Rows[0]["EmployeeCode"].ToString() + ") For Second Half  Dated On" + leaveApplication.FromDate.ToString("dd-MMM-yyyy") +
                                        ". Please go to the portal for Approving." +
                                        "<br> <br> <br>" +
                                        "Thank you";
                    }
                }
                else
                {
                    string dt = "";
                    dt = leaveApplication.ToDate != null ? leaveApplication.ToDate.Value.ToString("dd-MMM-yyyy") : "";

                    mailMessage = @"Dear " + responsiblePersonName + "<br> <br> <br>" +
                                        " You have a Leave Approval request of " + dtEmpInfo.Rows[0]["EmployeeName"].ToString() + "(" + dtEmpInfo.Rows[0]["EmployeeCode"].ToString() + ") For " + leaveDays + " Days,  Dated From " + leaveApplication.FromDate.ToString("dd-MMM-yyyy") + " To " + dt +
                                        ". Please go to the portal for Approving." +
                                        "<br> <br> <br>" +
                                        "Thank you";
                }
                _mailSenderService.SendFirstLeaveApproveRequestMail(responsiblePersonId, identity.PlantId, mailMessage, responsiplePersonEmail, responsiblePersonName, leaveApplication.EmpSystemID, dtEmpInfo.Rows[0]["EmployeeName"].ToString(), dtEmpInfo.Rows[0]["EmployeeCode"].ToString());
            }
            #endregion
            return Json(new { LeaveApplication = leaveApplication, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
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

        [HttpPost, Authorize]
        public JsonResult Edit(LeaveTransaction leaveApplication, string yearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            leaveApplication.UpdatedBy = identity.Name;
            _leaveTransactionService.SaveData(leaveApplication, yearId);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _leaveTransactionService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });

        }
        [HttpPost, Authorize]
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
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
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