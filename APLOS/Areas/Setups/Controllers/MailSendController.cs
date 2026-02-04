#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Service.Setups;
using System;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class MailSendController : BaseController
    {
        #region Constructor

        private readonly IMailReceiverService _mailReceiverService;
        private readonly IMailSenderService _mailSenderService;
        private readonly IResignationService _resignationService;


        public MailSendController(IMailReceiverService mailReceiverService
            , IMailSenderService mailSenderService, IResignationService resignationService)
        {
            _mailReceiverService = mailReceiverService;
            _mailSenderService = mailSenderService;
            _resignationService = resignationService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public JsonResult ApprovedEmployeeList()
        {
            _mailSenderService.SendProbationApprovedEmployeeList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult SendIncrementDueEmployeeList()
        {
            _mailSenderService.SendIncrementDueEmployeeList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult ApprovedForResignationEmployeeList()
        {
            _mailSenderService.SendApprovedResignedEmployeeList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult PreApprovedEmployeeList()
        {
            _mailSenderService.SendPreApprovedEmployeeList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult ProbationPeriodList()
        {
            _mailSenderService.SendProbationPeriodList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult AppliedResignationEmployeeList()
        {
            _mailSenderService.SendAppliedResignationEmployeeList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult ResignationDueList()
        {
            _mailSenderService.SendResignationDueList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult SaparatedEmployeeList()
        {
            _resignationService.UpdateResignedEmployees();
            //_mailSenderService.SendSaparatedEmployeeList("TS", "TS", "10215");

            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult ResignationToBeApprovedList()
        {
            _mailSenderService.SendResignationToBeApprovedList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult DailyAttendanceReport()
        {
            _mailSenderService.SendDailyAttendanceNotificationList("TS", "TS", "10215",DateTime.Now.ToString("dd-MMM-yyyy"));
            return Json(new { Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult DailyMissedPunchReport()
        {
            _mailSenderService.SendDailyMissedPunchEmpList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//SendDailyAttendanceFromAppList
        [HttpPost, Authorize]
        public JsonResult DailyAttendanceFromAppReport()
        {
            _mailSenderService.SendDailyAttendanceFromAppList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//
        [HttpPost, Authorize]
        public JsonResult SendEmpApprovalMailReport()
        {
            _mailSenderService.SendEmployeeApprovalAppList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//
        [HttpPost, Authorize]
        public JsonResult SendManualAttendanceEmployeeList()
        {
            _mailSenderService.SendManualAttendanceEmployeeList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//
        [HttpPost, Authorize]
        public JsonResult SendDailyDevicePunchList()
        {
            _mailSenderService.SendDailyDevicePunchList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//
        [HttpPost, Authorize]
        public JsonResult SendDailyAttendanceSummary()
        {
            _mailSenderService.SendDailyAttendanceSummary("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult SendYesterdayAbsentNotificationList()
        {
            _mailSenderService.SendYesterdayAbsentNotificationList("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult SendYesterdayMissedPunchNotificationList()
        {
            _mailSenderService.SendYesterdayMissedPunchReport("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult SendMonthlyAttendanceInformationReport()
        {
            _mailSenderService.SendMonthlyAttendanceInformationReport("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult SendDailyAttendanceAuditReport()
        {
            _mailSenderService.SendDailyAttendanceAuditReport("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }
        [HttpPost, Authorize]
        public JsonResult SendDailyProductionReport()
        {
            _mailSenderService.SendDailyProductionReport("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult SendLateAttendancePosting()
        {
            _mailSenderService.SendLateAttendancePosting("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//SendLateAttendancePosting..

        [HttpPost, Authorize]
        public JsonResult SendRunTaskNotification(string companyId)
        {
            _mailSenderService.RunTaskNotification("TS", "TS", "10215", companyId);
            return Json(new { Message = AplosMessage.Success });
        }//SendLateAttendancePosting

        [HttpPost, Authorize]
        public JsonResult SendAccountDelayPosting(string companyId)
        {
            _mailSenderService.SendAccountDelayPosting("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//SendLateAttendancePosting         
        [HttpPost, Authorize]
        public JsonResult SendYestedayOverstayMail()
        {
            _mailSenderService.SendYestedayOverstayMail("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//SendLateAttendancePosting 
        [HttpPost, Authorize]
        public JsonResult SendDailyAttendanceSummaryPositonWiseMail()
        {
            _mailSenderService.SendDailyAttendanceSummaryPositonWiseMail("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//SendLateAttendancePosting 

        [HttpPost, Authorize]
        public JsonResult SendTNAReportMail()
        {
            //_mailSenderService.SendTNAReportMail("TS", "TS", "10215");
            _mailSenderService.RunTNAScheduler("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//SendLateAttendancePosting 
        [HttpPost, Authorize]
        public JsonResult SendLastFewDaysPayableCreatedMail()
        {
            _mailSenderService.SendLastFewDaysPayableCreatedMail("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//SendLateAttendancePosting 
        [HttpPost, Authorize]
        public JsonResult SendLastFewDaysPaymentMadeMail()
        {
            _mailSenderService.SendLastFewDaysPaymentMadeMail("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//SendLateAttendancePosting 

        [HttpPost, Authorize]
        public JsonResult SaveScandataToBooking()
        {
            _mailSenderService.SendControlChart("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//SaveScandataToBookingforPacking 

        [HttpPost, Authorize]
        public JsonResult LVProcess()
        {
            _mailSenderService.LVProcess("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//LVProcessOLD 

        [HttpPost, Authorize]
        public JsonResult SavePendingBankReconciliation()
        {
            _mailSenderService.SendPendingBankReconciliationCreatedMail("TS", "TS", "10215");
            return Json(new { Message = AplosMessage.Success });
        }//SaveScandataToBookingforPacking 
    }
}