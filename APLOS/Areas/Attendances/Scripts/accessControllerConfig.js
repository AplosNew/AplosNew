AccessControllerConfig.$inject = ['$routeProvider', '$locationProvider', '$windowProvider'];
function AccessControllerConfig($routeProvider, $locationProvider, $windowProvider) {
    $routeProvider
        .when('/biometric-device-access-list', {
            templateUrl: 'Attendances/accesscontrollerlist/Aplos',
            controller: 'biometricDeviceAsAccessListController'
        })
        .when('/employee-device-access', {
            templateUrl: 'Attendances/accesscontrolleremployeetag/Aplos',
            controller: 'accessControllerEmployeeTagController'
        })
        .when('/device-as-short-leave', {
            templateUrl: 'Attendances/biometricdeviceasshortleave/Aplos',
            controller: 'biometricDeviceAsShortLeaveController'
        })
        .when('/employee-device', {
            templateUrl: 'Attendances/AccessControllerEmployeeTag/EmployeeDevice',
            controller: 'employeeDeviceController'
        })
        .when('/ot-management', {
            templateUrl: 'Attendances/OTManagement/OTConfirmation',
            controller: 'OTManagementController'
        })
        .when('/attendance-raw-data-delete', {
            templateUrl: 'Attendances/AttendanceRawDataDelete/Aplos',
            controller: 'AttendanceRawDataDeleteController'
        })
        .when('/attendance-raw-data-delete-new', {
            templateUrl: 'Attendances/AttendanceRawDataDeleteNew/Aplos',
            controller: 'AttendanceRawDataDeleteNewController'
        })
        .when('/attendance-raw-data-upload', {
            templateUrl: 'Attendances/AttendanceRawDataUpload/Aplos',
            controller: 'AttendanceRawDataUploadController'
        })
        .when('/employee-profile-upload', {
            templateUrl: 'Attendances/EmployeeProfileUpload/Aplos',
            controller: 'EmployeeProfileUploadController'
        })
        .when('/leave-year-end-process', {
            templateUrl: 'Attendances/LeaveYearEndProcess/Aplos',
            controller: 'LeaveYearEndProcessController'
        })
        .when('/leave-year-end-process-new', {
            templateUrl: 'Attendances/LeaveYearEndProcess/Aplos',
            controller: 'LeaveYearEndProcessNewController'
        })
        .when('/leave-year-end-encashment', {
            templateUrl: 'Attendances/LeaveYearEndProcess/Approval',
            controller: 'LeaveYearEndProcessEncashmentApprovalController'
        })
        .when('/shift-roster-creation', {
            templateUrl: 'Attendances/ShiftRosterCreation/Aplos',
            controller: 'ShiftRosterCreationController'
        })
        .when('/shift-assignment', {
            templateUrl: 'Attendances/ShiftAssignment/Aplos',
            controller: 'shiftAssignmentController'
        })
        .when('/department-group', {
            templateUrl: 'Attendances/DepartmentGroup/Aplos',
            controller: 'DepartmentGroupController'
        })
        .when('/shift-creation', {
            templateUrl: 'Attendances/ShiftCreation/ShiftCreation',
            controller: 'shiftCreationController'
        })
        .when('/sandwich-absent', {
            templateUrl: 'Attendances/SandwichAbsent/Aplos',
            controller: 'SandwichAbsentController'
        })
        .when('/extra-ot', {
            templateUrl: 'Attendances/ExtraOT/Aplos',
            controller: 'ExtraOTController'
        })
        .when('/extra-ot-delete', {
            templateUrl: 'Attendances/ExtraOTDelete/Aplos',
            controller: 'ExtraOTDeleteController'
        })
        .when('/monthly-attendance-information', {
            templateUrl: 'Attendances/AttendanceProcessUI/MonthlyAttendanceInformation',
            controller: 'monthlyAttendanceInformationController'
        })
        .when('/monthly-attendance-information-new', {
            templateUrl: 'Attendances/MonthlyAttendanceInformationNew/Aplos',
            controller: 'MonthlyAttendanceInformationNewController'
        })
        .when('/monthly-attendance-information-all', {
            templateUrl: 'Attendances/AttendanceProcessUI/MonthlyAttendanceInformationALLStatus',
            controller: 'monthlyAttendanceInformationController'
        })
        .when('/monthly-attendance-information-all-new', {
            templateUrl: 'Attendances/MonthlyAttendanceInformationNew/MonthlyInfoAll',
            controller: 'MonthlyAttendanceInformationNewController'
        })
        .when('/monthly-attendance-information-date-range', {
            templateUrl: 'Attendances/AttendanceProcessUI/MonthlyAttendanceInformationDateRange',
            controller: 'monthlyAttendanceInformationDateRangeController'
        })
       .when('/monthly-attendance-information-date-range-new', {
            templateUrl: 'Attendances/MonthlyAttendanceInformationNew/MonthlyInfoDateRange',
            controller: 'MonthlyAttendanceInformationDateRangeNewController'
        })
        .when('/individual-fixed-ot', {
            templateUrl: 'Attendances/InvididualFixedOT/Aplos',
            controller: 'individualFixedOTController'
        })
        .when('/raw-data-download', {//remarks: raw table data
            templateUrl: 'Attendances/RawDataDownload/Aplos',
            controller: 'rawDataDownloadController'
        })
        .when('/raw-data-download-text', {// remarks: based on processed data
            templateUrl: 'Attendances/ComplianceRawDataDownload/Aplos',
            controller: 'complianceRawDataDownloadController'
        })
        .when('/device-raw-data-download-text', {//remarks : based on settting
            templateUrl: 'Attendances/DeviceRawDataDownload/Aplos',
            controller: 'DeviceRawDataDownloadController'
        })
        .when('/attendance-process', {
            templateUrl: 'Attendances/AttendanceProcessUI/Aplos',
            controller: 'attendanceProcessUIController'
        })
        .when('/employee-wise-fixed-ot-setting', {
            templateUrl: 'Attendances/EmployeeWiseFixedOTSetting/Aplos',
            controller: 'employeeWiseFixedOTSettingController'
        })
        .when('/shift-time-change', {
            templateUrl: 'Attendances/ShiftTimeChange/Aplos',
            controller: 'shiftTimeChangeController'
        })
        .when('/attendance-bonus-policy', {
            templateUrl: 'Attendances/AttendanceBonusPolicy/Aplos',
            controller: 'attendanceBonusPolicyController'
        })
        .when('/raw-data-set-inout', {
            templateUrl: 'Attendances/RawDataSetInOut/Aplos',
            controller: 'rawDataSetInOutController'
        })
        .when('/daily-attendance-status-report', {
            templateUrl: 'Attendances/DailyAttendanceStatusReport',
            controller: 'dailyAttendanceStatusReportController'
        })
        .when('/ot-final-information', {
            templateUrl: 'Attendances/AttendanceProcessUI/OTFinalInformation',
            controller: 'otFinalInformationController'
        })
        .when('/ot-final-information-new', {
            templateUrl: 'Attendances/OTFinalInformationNew/Aplos',
            controller: 'otFinalInformationNewController'
        })
        .when('/attendance-entry', {
            templateUrl: 'Attendances/AttendanceEntry/Aplos',
            controller: 'attendanceEntryController'
        })
        .when('/tbs-transctation', {
            templateUrl: 'Attendances/TBS/Aplos',
            controller: 'tBSController'
        })
        .when('/shift-assignment-delete', {
            templateUrl: 'Attendances/ShiftAssignmentDelete/Aplos',
            controller: 'shiftAssignmentDeleteController'
        })
        .when('/attendance-on-day-status', {
            templateUrl: 'Attendances/AttendanceOnDayStatus/Aplos',
            controller: 'attendanceOnDayStatusController'
        })
        .when('/bonus-policy', {
            templateUrl: 'Attendances/BonusPolicy/Aplos',
            controller: 'bonusPolicyController'
        })
        .when('/daily-allowance-report', {
            templateUrl: 'Attendances/DailyAllowance/Aplos',
            controller: 'dailyAllowanceController'
        })
        .when('/job-location', {
            templateUrl: 'Attendances/JobLocation/Aplos',
            controller: 'jobLocationController'
        })
        .when('/ot-slab', {
            templateUrl: 'Attendances/OTSlab/Aplos',
            controller: 'otSlabController'
        })
        .when('/gratuity-policy', {
            templateUrl: 'Attendances/GratuityPolicy/Aplos',
            controller: 'gratuityPolicyController'
        })
        .when('/daily-allowance-summary', {
            templateUrl: 'Attendances/DailyAllowance/DailyAllowanceSummary',
            controller: 'dailyAllowanceController'
        })
        .when('/compliance-attendance-setting', {
            templateUrl: 'Attendances/ComplianceAttendanceSetting/Aplos',
            controller: 'complianceAttendanceSettingController'
        })
        .when('/nce-job-card', {
            templateUrl: 'Attendances/ComplianceAttendanceSetting/JobCardCompliance',
            controller: 'jobCardcomplianceReportController'
        })
        .when('/nnce-job-card', {
            templateUrl: 'Attendances/ComplianceAttendanceSetting/BuyerJobCardCompliance',
            controller: 'BuyerjobCardcomplianceReportController'
        })
        .when('/salary-head-wise-amount', {
            templateUrl: 'Attendances/SalaryHeadWiseAmountSetting/Aplos',
            controller: 'salaryHeadWiseAmountSettingController'
        })
        .when('/pf-policy', {
            templateUrl: 'Attendances/PFPolicy/Aplos',
            controller: 'PFPolicyController'
        })
        .when('/slab-salary', {
            templateUrl: 'Attendances/SalarySlabWiseValue/Aplos',
            controller: 'salarySlabWiseValueController'
        })
       
        .when('/holiday-pay-day', {
            templateUrl: 'Attendances/AdditionalPayDay/Aplos',
            controller: 'AdditionalPayDayController'
        })

        .when('/device-raw-data-download-text', {
            templateUrl: 'Attendances/DeviceRawDataDownload/Aplos',
            controller: 'DeviceRawDataDownloadController'
        })

        .when('/ot-policy', {
            templateUrl: 'Attendances/OTPolicy/Aplos',
            controller: 'OTPolicyController'
        })
        .when('/ot-limit-transaction', {
            templateUrl: 'Attendances/OTLimitTransaction/Aplos',
            controller: 'OTLimitTransactionController'
        })
        .when('/ot-limit-transaction-from-app', {
            templateUrl: 'Attendances/OTLimitTransactionFromApp/Aplos',
            controller: 'OTLimitTransactionFromAppController'
        })
        .when('/ot-limit-setting', {
            templateUrl: 'Attendances/OTLimitSetting/Aplos',
            controller: 'OTLimitSettingController'
        })

        .when('/monthly-retain-bonus', {
            templateUrl: 'Attendances/BonusPolicyMonthlyRetain/Aplos',
            controller: 'BonusPolicyMonthlyRetainController'
        })

        .when('/missed-punch-report', {
            templateUrl: 'Attendances/MissedPunchReport/Aplos',
            controller: 'MissedPunchReportController'
        })

        .when('/esic-policy', {
            templateUrl: 'Attendances/ESICPolicy/Aplos',
            controller: 'ESICPolicyController'
        })

        .when('/daily-attandance-information', {
            templateUrl: 'Attendances/DailyAttendanceInformation/Aplos',
            controller: 'DailyAttendanceInformationController'
        })

        .when('/daily-attandance-summery-report', {
            templateUrl: 'Attendances/DailyAttendanceSummeryReport/Aplos',
            controller: 'DailyAttendanceSummeryReportController'
        })

        .when('/monthly-attandance-summery-report', {
            templateUrl: 'Attendances/MonthlyAttendanceSummeryReport/Aplos',
            controller: 'MonthlyAttendanceSummeryReportController'
        })

        .when('/attendance-manual-data-upload', {
            templateUrl: 'Attendances/AttendanceManualDataUpload/Aplos',
            controller: 'AttendanceManualDataUploadController'
        })

        .when('/final-attendance-process', {
            templateUrl: 'Attendances/FinalAttendanceProcess/Aplos',
            controller: 'FinalAttendanceProcessController'
        })

        .when('/manual-attendance-file-upload', {
            templateUrl: 'Attendances/ManualAttendanceFileUpload/Aplos',
            controller: 'ManualAttendanceFileUploadController'
        })

        .when('/manual-attendance-with-shift', {
            templateUrl: 'Attendances/ManualAttendanceWithShift/Aplos',
            controller: 'ManualAttendanceWithShiftController'
        })

        .when('/daily-attendance-status-rpt', {
            templateUrl: 'Attendances/DailyAttendanceStatusRpt/Aplos',
            controller: 'DailyAttendanceStatusRptController'
        })
        .when('/monthly-lunchout-report', {
            templateUrl: 'Attendances/MonthlyLunchOutReport/Aplos',
            controller: 'MonthlyLunchOutReportController'
        })
        .when('/ot-manual', {
            templateUrl: 'Attendances/OTManual/Aplos',
            controller: 'OTManualController'
        })
        .when('/manual-ot-upload', {
            templateUrl: 'Attendances/ManualOTUpload/Aplos',
            controller: 'ManualOTUploadController'
        })

        .when('/lunchout-dashboard', {
            templateUrl: 'Attendances/LunchOutDashboard/Aplos',
            controller: 'LunchOutDashboardController'
        })

        .when('/manual-ot-report', {
            templateUrl: 'Attendances/ManualOTReport/Aplos',
            controller: 'ManualOTReportController'
        })

        .when('/daily-attendance-report', {
            templateUrl: 'Attendances/DailyAttendanceReport/Aplos',
            controller: 'DailyAttendanceReportController'
        })
        .when('/attnd-report', {
            templateUrl: 'Attendances/AttendanceFromAppReport/Aplos',
            controller: 'AttendanceFromAppReportController'
        })
        .when('/multiple-emp-attdn-lock', {
            templateUrl: 'Attendances/MultipleEmployeeIndividualLock/Aplos',
            controller: 'MultipleEmployeeIndividualLockController'
        })
        .when('/year-present-days-summary', {
            templateUrl: 'Attendances/EntireYearPresentDaysSummary/Aplos',
            controller: 'EntireYearPresentDaysSummaryController'
        })
        .when('/balance-ot-report', {
            templateUrl: 'Attendances/BalanceOTReport/Aplos',
            controller: 'BalanceOTReportController'
        })
        .when('/emp-last-punch-report', {
            templateUrl: 'Attendances/EmployeeLastPunchReport/Aplos',
            controller: 'EmployeeLastPunchReportController'
        })
        .when('/monthly-good-work-report', {
            templateUrl: 'Attendances/MonthlyGoodWorkReport/Aplos',
            controller: 'monthlyGoodWorkReportController'
        })
        .when('/monthly-good-work-report-new', {
            templateUrl: 'Attendances/MonthlyGoodWorkReportNew/Aplos',
            controller: 'monthlyGoodWorkReportNewController'
        })
        .when('/weekoff-extraot-report', {
            templateUrl: 'Attendances/WeekOffHolidayOTReport/Aplos',
            controller: 'weekOffOTReportController'
        })
        .when('/weekoff-extraot-report-original', {
            templateUrl: 'Attendances/WeekOffHolidayOTReport/Aplos',
            controller: 'weekOffOTReportOriginalController'
        })
        .when('/holiday-extraot-report', {
            templateUrl: 'Attendances/WeekOffHolidayOTReport/HolidayOT',
            controller: 'holidayOTReportController'
        })
        .when('/holiday-extraot-report-original', {
            templateUrl: 'Attendances/WeekOffHolidayOTReport/HolidayOT',
            controller: 'holidayOTReportOriginalController'
        })
        .when('/exception-ot-process', {
            templateUrl: 'Attendances/ExceptionOTProcess/Aplos',
            controller: 'ExceptionOTProcessController'
        })
        .when('/audit-report-summery', {
            templateUrl: 'Attendances/AuditReportSummery/Aplos',
            controller: 'AuditReportSummeryController'
        })

        .when('/audit-report-summary-new', {
            templateUrl: 'Attendances/AuditReportSummaryNew/Aplos',
            controller: 'AuditReportSummaryNewController'
        })

        .when('/ot-manual-new', {
            templateUrl: 'Attendances/OTManualNew/Aplos',
            controller: 'OTManualNewController'
        })
        .when('/new-attnd-process', {
            templateUrl: 'Attendances/NewAttendanceProcess/Aplos',
            controller: 'NewAttendanceProcessController'
        })
        .when('/new-attnd-process-plantwise', {
            templateUrl: 'Attendances/NewAttendanceProcessPlantWise/Aplos',
            controller: 'NewAttendanceProcessPlantWiseController'
        })

        .when('/manual-ot-upload-new', {
            templateUrl: 'Attendances/ManualOTUploadNew/Aplos',
            controller: 'ManualOTUploadNewController'
        })

        .when('/manual-ot-report-new', {
            templateUrl: 'Attendances/ManualOTReportNew/Aplos',
            controller: 'ManualOTReportNewController'
        })
        .when('/payroll-management-dashboard', {
            templateUrl: 'Attendances/PayrollManagementDashboard/Aplos',
            controller: 'PayrollManagementDashboardController'
        })
        .when('/attendance-raw-data-from-app', {
            templateUrl: 'Attendances/AttendanceRawDataFromApp/Aplos',
            controller: 'AttendanceRawDataFromAppController'
        })
        .when('/attendance-dashboard', {
            templateUrl: 'Attendances/AttendanceDashboard/Aplos',
            controller: 'AttendanceDashboardController'
        })
        .when('/new-attndprocess-reprocess', {
            templateUrl: 'Attendances/NewProcessAttendanceReProcess/Aplos',
            controller: 'NewProcessAttendanceReProcessController'
        })
        .when('/sandwich-process', {
            templateUrl: 'Attendances/SandwichProcess/Aplos',
            controller: 'SandwichProcessController'
        })
        .when('/sandwich-process-plantwise', {
            templateUrl: 'Attendances/SandwichProcess/PlantWise',
            controller: 'SandwichProcessPlantWiseController'
        })
        .when('/monthly-leave-balance', {
            templateUrl: 'Attendances/MonthlyLeaveBalance/Aplos',
            controller: 'MonthlyLeaveBalanceController'
        })
        .when('/eot', {
            templateUrl: 'Attendances/EOT/Aplos',
            controller: 'EOTController'
        })
        .when('/good-work', {
            templateUrl: 'Attendances/GoodWork/Aplos',
            controller: 'GoodWorkController'
        })
        .when('/gw-datechaange', {
            templateUrl: 'Attendances/GoodWork/DateChange',
            controller: 'GoodWorkDateChangeController'
        })
        .when('/payable-creation-employee-advance', {
            templateUrl: 'Attendances/GoodWork/PCAAC',
            controller: 'PayableCreationAndWorkerAdvanceController'
        })
        .when('/employee-multiple-advance', {
            templateUrl: 'Attendances/GoodWork/EmployeeMultipleAdvance',
            controller: 'EmployeeMultipleAdvanceController'
        })
        .when('/goodworksetup', {
            templateUrl: 'Attendances/GoodWorkSetup',
            controller: 'GoodWorkSetupController'
        })
        .when('/good-work-check', {
            templateUrl: 'Attendances/GoodWork/GoodWorkCheck',
            controller: 'GoodWorkCheckedController'
        })
        .when('/good-work-approve', {
            templateUrl: 'Attendances/GoodWork/GoodWorkApprove',
            controller: 'GoodWorkApproveController'
        })
        .when('/good-work-payment-disburse', {
            templateUrl: 'Attendances/GoodWork/GWPaymnetDisburse',
            controller: 'GoodWorkPaymentDisburseController'
        })
        .when('/gw-report', {
            templateUrl: 'Attendances/GoodWork/GWReport',
            controller: 'GoodWorkReportController'
        })
        .when('/sd-approve', {
            templateUrl: 'Attendances/SpecialDuty/Aplos',
            controller: 'SpecialDutyController'
        })
        ;
} 