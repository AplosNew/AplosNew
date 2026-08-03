using Syncfusion.XlsIO;

namespace Library.Service.Setups
{
    public interface IMailSenderService
    {
        /// <summary>
        /// Probation Confirmed
        /// </summary>
        void SendPreApprovedEmployeeList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Employees in Probation Period
        /// </summary>
        void SendProbationPeriodList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Daily Approved Employees from Probation Period
        /// </summary>
        void SendProbationApprovedEmployeeList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Daily Applied for Resignation
        /// </summary>
        void SendAppliedResignationEmployeeList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Daily Approved Salary Rules
        /// </summary>
        void SendApprovedSalaryProcessNotification(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Daily Approved Resignations by system or authority.
        /// </summary>
        void SendApprovedResignedEmployeeList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// List of Employees waiting for Leave as per Resignation Approved Date.
        /// </summary>
        void SendResignationDueList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// List of Panding Employees to be Approved
        /// </summary>
        void SendResignationToBeApprovedList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// List of To be Separated Employees
        /// </summary>
        void SendSaparatedEmployeeList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Increment Due Employee List
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendIncrementDueEmployeeList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Daily Attendance Process
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendDailyAttendanceNotificationList(string addedBy, string ip, string appVersion, string workDate);

        /// <summary>
        /// Daily Missed Punch Report
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendDailyMissedPunchEmpList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Daily Attendance From App  Report
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendDailyAttendanceFromAppList(string addedBy, string ip, string appVersion);
        void SendEmployeeApprovalAppList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Manual Attendance Notification
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendManualAttendanceEmployeeList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Daily Device Punch Information
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendDailyDevicePunchList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Daily Attendance Summary
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendDailyAttendanceSummary(string addedBy, string ip, string appVersion);

        /// <summary>
        /// YesterDay Absent Summary
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendYesterdayAbsentNotificationList(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Yesterday Punch Summary
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendYesterdayMissedPunchReport(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Monthly Attendance Information (Details with Color)
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendMonthlyAttendanceInformationReport(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Daily Attendance Audit
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendDailyAttendanceAuditReport(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Daily Production Report
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendDailyProductionReport(string addedBy, string ip, string appVersion);

        /// <summary>
        /// To Do schedular Date and time processing/Updating
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void RunTodoScheduler(string addedBy, string ip, string appVersion);


        void SendLateAttendancePosting(string addedBy, string ip, string appVersion);

        void RunTaskNotification(string addedBy, string ip, string appVersion, string companyGroupId);
        void RunTNAScheduler(string addedBy, string ip, string appVersion);

        /// <summary>
        /// Account Delay Posting
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendAccountDelayPosting(string addedBy, string ip, string appVersion);
        /// <summary>
        /// Yesterday Overstay
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendYestedayOverstayMail(string addedBy, string ip, string appVersion);

        /// <summary>
        /// For Download Report From Screen
        /// </summary>
        /// <param name="companyGroupId"></param>
        /// <param name="plantId"></param>
        /// <param name="SheetHeader"></param>
        /// <param name="SheetName"></param>
        /// <returns></returns>
        string GetDailyAttendanceEmpInfoList(string companyGroupId, string companyId, string plantId, string SheetHeader, string SheetName, string workDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec,string empCategoryList,string designationList,string lineList,string Dstatus);
        string GetDailyAttendanceEmpInfo(string companyGroupId, string companyId, string plantId, string SheetHeader, string SheetName, string workDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec,string empCategoryList,string designationList,string lineList,string Dstatus,bool WithFatherName, string JobLocation);
        /// <summary>
        /// Position Wise Grouping
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendDailyAttendanceSummaryPositonWiseMail(string addedBy, string ip, string appVersion);

        /// <summary>
        /// DailyProductionTargetSchedule
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void DailyProductionTargetSchedule(string addedBy, string ip, string appVersion);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addedBy"></param>
        /// <param name="ip"></param>
        /// <param name="appVersion"></param>
        void SendControlChart(string addedBy, string ip, string appVersion);

        void SendFirstLeaveApproveRequestMail(string empSystemId, string plantId, string emailMessage, string toEmailId, string toEmailEmployeeName, string laEmpSystemId, string laEmpName, string laEmpCode);
        IWorkbook GetDailyAttendanceDataForView(string companyGroupId, string companyId, string plantId, string SheetHeader, string SheetName, string workDate);

        void SendTNAReportMail(string addedBy, string ip, string appVersion);
        void SendLastFewDaysPayableCreatedMail(string addedBy, string ip, string appVersion);
        void SendLastFewDaysPaymentMadeMail(string addedBy, string ip, string appVersion);
        void SaveScandataToBooking(string addedBy, string ip, string appVersion);
        void SaveInspectionToBooking(string addedBy, string ip, string appVersion);
        void SendPendingBankReconciliationCreatedMail(string addedBy, string ip, string appVersion);
        void LVProcess(string addedBy, string ip, string appVersion);
    }
}