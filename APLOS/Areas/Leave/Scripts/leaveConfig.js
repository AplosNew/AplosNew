leaveConfig.$inject = ['$routeProvider'];
function leaveConfig($routeProvider) {
    $routeProvider     

        .when('/leave-policy', {
            templateUrl: 'Leave/LeavePolicy/LeavePolicy',
            controller: "leavePolicyController"
        })
        .when('/employee-leave-approval', {
            templateUrl: 'Leave/EmployeeLeaveApproval/Aplos',
            controller: "EmployeeLeaveApprovalController"
        })
        .when('/off-duty-hours', {
            templateUrl: 'Leave/OffDutyHours/Aplos',
            controller: "offDutyHoursController"
        })
        .when('/off-duty-hours-approval', {
            templateUrl: 'Leave/OffDutyApprove/Aplos',
            controller: "offDutyApproveController"
        })
        .when('/hourly-off-duty-tag', {
            templateUrl: 'Leave/HourlyOffDutyTag/Aplos',
            controller: "hourlyOffDutyTagController"
        })
        .when('/hourly-ot', {
            templateUrl: 'Leave/HourlyOT/Aplos',
            controller: "hourlyOTController"
        })
        .when('/hourly-leave-reason', {
            templateUrl: 'Leave/HourlyLeaveReason/Aplos',
            controller: "hourlyLeaveReasonController"
        })
        .when('/hourly-ot-report', {
            templateUrl: 'Leave/HourlyOT/HourlyOtReport',
            controller: "hourlyOTController"
        })
        .when('/hourly-leave-report', {
            templateUrl: 'Leave/OffDutyHours/OffDutyHoursReport',
            controller: "offDutyHoursController"
        })
        .when('/monthly-hourly-ot', {
            templateUrl: 'Leave/HourlyOT/HourlyOtReportMonth',
            controller: "hourlyOTController"
        })
        .when('/individual-daily-ot', {
            templateUrl: 'Leave/HourlyOT/IndividualDailyOt',
            controller: "hourlyOTController"
        })
        .when('/on-duty-transaction', {
            templateUrl: 'Leave/OnDutyTransaction/Aplos',
            controller: "onDutyTransactionController"
        })
        .when('/on-duty-approval', {
            templateUrl: 'Leave/OnDutyApproval/Aplos',
            controller: "onDutyApprovalController"
        })
        .when('/leave-type', {
            templateUrl: 'Leave/LeaveType/Aplos',
            controller: "leaveTypeController"
        })
        .when('/emp-bank-info-information', {
            templateUrl: 'Leave/EmployeeBankInfoInformation/Aplos',
            controller: "EmployeeBankInfoInformationController"
        })

        .when('/sandwich-leaveon-holiday', {
            templateUrl: 'Leave/SandWichLeaveOnHoliday/Aplos',
            controller: "SandWichLeaveOnHolidayController"
        })
        .when('/earn-leave-pay-slip', {
            templateUrl: 'Leave/EarnLeavePaySlip/Aplos',
            controller: "EarnLeavePaySlipController"
        })
        .when('/c-ot-report', {
            templateUrl: 'Leave/CHourlyOTReport/Aplos',
            controller: "chourlyOTReportController"
        })

        .when("/first-auth-employee-leave-approval", {
            templateUrl: "Employees/FirstAuthEmpLeaveApproval",
            controller: "firstAuthEmpLeaveApprovalController"
        })
        .when('/ca-ot-report', {
            templateUrl: 'Leave/CAHourlyOTReport/Aplos',
            controller: "cahourlyOTReportController"
        })
        .when('/leave-register-todate-report', {
            templateUrl: 'Leave/LeaveBalanceToDateReport/Aplos',
            controller: "LeaveBalanceToDateReportController"
        })
        .when('/leave-register-report', {
            templateUrl: 'Leave/LeaveBalanceReport/Aplos',
            controller: "LeaveBalanceReportController"
        })

        .when('/leave-register-todate-report', {
            templateUrl: 'Leave/LeaveBalanceToDateReport/Aplos',
            controller: "LeaveBalanceToDateReportController"
        })
        ;
}