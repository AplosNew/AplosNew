/// <reference path="../../../scripts/angular-cbo-factory.js" />
HumanResourceConfig.$inject = ['$routeProvider', '$locationProvider'];
function HumanResourceConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/legal-salary-grade', {
            templateUrl: 'humanresource/legalsalarygrade',
            controller: 'legalSalaryGradeController'
        })
        .when('/legal-salary-grade-designation', {
            templateUrl: 'humanresource/legalsalarygradedesignation',
            controller: 'legalSalaryGradeDesignationController'
        })
        .when('/legal-salary-structure', {
            templateUrl: 'humanresource/legalsalarystructure',
            controller: 'legalSalaryStructureController'
        })
        .when('/legal-salary-report', {
            templateUrl: 'humanresource/legalsalarystructure/legalsalaryreportpage',
            controller: 'legalSalaryReportController'
        })
        .when('/salary-fixation-setting', {
            templateUrl: 'humanresource/salaryfixationsetting',
            controller: 'salaryFixationSettingController'
        })
        .when('/short-leave-policy', {
            templateUrl: 'humanresource/shortleavepolicy',
            controller: 'shortLeavePolicyController'
        })
        .when('/salary-fixation', {
            templateUrl: 'humanresource/salaryfixation',
            controller: 'salaryFixationController'
        })
        .when('/salary-fixation-mail', {
            templateUrl: 'humanresource/salaryfixation/aplosmail',
            controller: 'salaryFixationController'
        })
        .when('/designation-configuration', {
            templateUrl: 'humanresource/designationMasterConfiguration/aplos',
            controller: 'designationMasterConfigurationController'
        })
        .when('/annual-non-cash', {
            templateUrl: 'humanresource/annualnoncash',
            controller: 'annualNonCashController'
        })
        .when('/disciplinary-action-category', {
            templateUrl: 'humanresource/DisciplinaryActionCategory/aplos',
            controller: 'disciplinaryActionCategoryController'
        })
        .when('/employee-disciplinary-action', {
            templateUrl: 'humanresource/employeeDisciplinaryAction/aplos',
            controller: 'employeeDisciplinaryActionController'
        })
        .when('/disciplinary-action-transaction', {
            templateUrl: 'humanresource/employeeDisciplinaryAction/DisciplinaryActionTransaction',
            controller: 'employeeDisciplinaryActionTransactionController'
        })
        .when('/employee-shift-assign', {
            templateUrl: 'humanresource/EmployeeShiftAssign/aplos',
            controller: 'employeeShiftAssignController'
        })
        .when('/work-Group', {
            templateUrl: 'humanresource/WorkGroup/aplos',
            controller: 'workGroupController'
        })
        .when('/maternity-leave-policy', {
            templateUrl: 'humanresource/maternityleavepolicy/MaternityLeavePolicyNew',
            controller: 'maternityLeavePolicyNewController'
        })

        .when('/maternity-leave-transaction', {
            templateUrl: 'humanresource/MaternityLeaveTransaction/Aplos',
            controller: 'MaternityLeaveTransactionController'
        })
        .when('/rest', {
            templateUrl: 'humanresource/rest/aplos',
            controller: 'restController'
        })
        .when('/complied-shift', {
            templateUrl: 'humanresource/CompliedShift/aplos',
            controller: 'compliedshiftController'
        })
        .when('/complied-shift-Grouping', {
            templateUrl: 'humanresource/CompliedShiftGrouping/aplos',
            controller: 'compliedShiftGroupingController'
        })
        .when('/complied-shift-assignment', {
            templateUrl: 'humanresource/compliedshiftassignment/Aplos',
            controller: 'compliedShiftAssignmentController'
        })
        .when('/complied-shift-rotation', {
            templateUrl: 'humanresource/CompliedShiftAssignment/ShiftRotation',
            controller: 'complianceShiftRotationController'
        })
        .when('/compliance-attendance-report', {
            templateUrl: 'humanresource/compliedshiftassignment/daily',
            controller: 'dailyComplianceReportController'
        })
        .when('/leave-report', {
            templateUrl: 'humanresource/maternityLeaveTransaction/leave',
            controller: 'leaveInformationController'
        })
        .when('/salary-report', {
            templateUrl: 'humanresource/SalaryReport',
            controller: 'salaryReportController'
        })
        .when('/leave-encashment', {
            templateUrl: 'humanresource/maternityLeaveTransaction/LvEncash',
            controller: 'leaveEncashmentController'
        })
        .when('/employee-attendance', {
            templateUrl: 'humanresource/AttendanceReport/attend',
            controller: 'attendanceReportController'
        })

        .when('/employee-promotion', {
            templateUrl: 'humanresource/employeepromotion/aplos',
            controller: 'employeePromotionController'
        })
        .when('/promotion', {
            templateUrl: 'humanresource/employeepromotion/promotion',
            controller: 'employeePromotionController'
        })
        .when('/promotion-new', {
            templateUrl: 'humanresource/EmployeepromotionNew/promotion',
            controller: 'employeePromotionNewController'
        })
        .when('/promotion-increment', {
            templateUrl: 'humanresource/EmployeePromotionAndIncrement/Aplos',
            controller: 'EmployeePromotionAndIncrementController'
        })
        .when('/salary-process-allowance', {
            templateUrl: 'humanresource/PayrollReports/SalaryProcessedReportCompliance',
            controller: 'salaryProcessedReportComplianceController'
        })


        .when('/welfare-summary-report', {
            templateUrl: 'humanresource/WelfareSummaryReport/aplos',
            controller: 'welfareSummaryReportController'
        })

        //.when('/salary-Register', {
        //    templateUrl: 'humanresource/PayRegisterBDReport/',
        //    controller: 'payRegisterBDReportController'
        //})
        .when('/special-followup-report', {
            templateUrl: 'humanresource/SpecialFollowUPReport/',
            controller: 'SpecialFollowUpReportController'
        })

        //.when('/bonus-sheet', {
        //    templateUrl: 'humanresource/BonusSheet/',
        //    controller: 'bonusSheetController'
        //})

        .when('/long-absenteeism', {
            templateUrl: 'humanresource/LongAbsenteeismAssign/Aplos',
            controller: 'longAbsenteeismAssignController'
        })


        .when('/bonus-sheet', {
            templateUrl: 'humanresource/SFBonusSheetReport/Aplos',
            controller: 'SFBonusSheetReportController'
        })
        .when('/bonus-sheet-grid', {
            templateUrl: 'humanresource/SFBonusSheetReport/GridAplos',
            controller: 'SFBonusSheetGridReportController'
        })
        .when('/manpower-attendance-summary', {
            templateUrl: 'humanresource/ManpowerAttendanceSummary/',
            controller: 'manpowerAttendanceSummaryController'
        })
        .when('/manpower-attendance-summary-new', {
            templateUrl: 'humanresource/ManpowerAttendanceSummary/AplosNew',
            controller: 'manpowerAttendanceSummaryControllerNew'
        })
        .when('/c-manpower-attendance-summary', {
            templateUrl: 'humanresource/ManpowerAttendanceSummary/CustomAttdnSummary',
            controller: 'manpowerAttendanceSummaryController'
        })

        .when('/manpower-attendanceGroup-summary', {
            templateUrl: 'humanresource/ManpowerAttendanceSummary/AttendanceGroup',
            controller: 'manpowerAttendanceGroupSummaryController'
        })

        .when('/shift-report', {
            templateUrl: 'humanresource/AttendanceManagement/',
            controller: 'attendanceManagementController'
        })

        .when('/individual-job-card', {// remarks: 
            templateUrl: 'humanresource/AttendanceManagement/JobCard',
            controller: 'jobCardReportController'
        })

        .when('/job-card', {// remarks
            templateUrl: 'humanresource/AttendanceManagement/ComplianceJobCard',
            controller: 'compliancejobCardReportController'
        })
        .when('/job-card-new', {
            templateUrl: 'humanresource/JobCardReportNew/Aplos',
            controller: 'jobCardReportNewController'
        })

        .when('/ot-final', {
            templateUrl: 'humanresource/AttendanceManagement/OtFinal',
            controller: 'otFinalController'
        })
        .when('/salary-top-sheet', {
            templateUrl: 'humanresource/SalaryTopSheet',
            controller: 'salaryTopSheetController'
        })
        .when('/dynamic-top-sheet', {
            templateUrl: 'humanresource/SalaryTopSheet/DynamicTopSheet',
            controller: 'dynamicSalaryTopSheetController'
        })

        .when('/top-sheet-details', {
            templateUrl: 'humanresource/SalaryTopSheet/TopSheetDetails',
            controller: 'dynamicSalaryTopSheetController'
        })

        .when('/daily-day-status', {
            templateUrl: 'humanresource/dailydaystatus/',
            controller: 'dailyDayStatusController'
        })

        .when('/pay-reg-bd-rep-struct', {
            templateUrl: 'humanresource/PayRegisterBDReportWithStructure/',
            controller: 'payRegisterBDReportWithStructureController'
        })
        .when('/attendance-lock', {
            templateUrl: 'humanresource/HrmsSettings/PlantWiseAttendanceLock',
            controller: 'PlantWiseAttendanceLockController'
        })
        .when('/attendance-unlock-plant', {
            templateUrl: 'humanresource/HrmsSettings/PlantWiseAttendanceUnLock',
            controller: 'PlantWiseAttendanceUnLockController'
        })
        //.when('/attendance-unlock', {
        //    templateUrl: 'humanresource/HrmsSettings/EmployeeAndPlantWiseAttendanceUnLock',
        //    controller: 'EmployeeAndPlantWiseAttendanceUnLockController'
        //})
        .when('/individual-attendance-lock', {
            templateUrl: 'humanresource/HrmsSettings/IndividualAttendanceLock',
            controller: 'IndividualAttendanceLockController'
        })
        .when('/attendance-unlock', {
            templateUrl: 'humanresource/HrmsSettings/DateRangeWiseAttendanceUnLock',
            controller: 'DateRangeWiseAttendanceUnLockController'
        })


        .when('/individual-attendance-unlock', {
            templateUrl: 'humanresource/HrmsSettings/IndividualAttendanceUnLock',
            controller: 'IndividualAttendanceUnLockController'
        })
        .when('/tbs-assign', {
            templateUrl: 'humanresource/TBSAssign/',
            controller: 'tbsAssignController'
        })
        .when('/attendance-process-data', {
            templateUrl: 'humanresource/AttendanceProcessData/',
            controller: 'attendanceProcessDataController'
        })
        .when('/attendance-process-data-new', {
            templateUrl: 'humanresource/AttendanceProcessDataNew/',
            controller: 'attendanceProcessDataNewController'
        })
        .when('/attendance-process-data-entity', {
            templateUrl: 'humanresource/AttendanceProcessDataEntityWise/',
            controller: 'attendanceProcessDataEntityWiseController'
        })
        .when('/attendance-process-data-entity-new', {
            templateUrl: 'humanresource/AttendanceProcessDataEntityWiseNew/',
            controller: 'attendanceProcessDataEntityWiseNewController'
        })
        .when('/weekoff-change', {
            templateUrl: 'humanresource/WeekOffChange/Aplos',
            controller: 'WeekOffChangeController'
        })
        .when('/trim-in-time', {
            templateUrl: 'humanresource/TrimInTime/Aplos',
            controller: 'TrimInTimeController'
        })
        .when('/manual-out-time', {
            templateUrl: 'humanresource/AttendanceManagement/ManualOutTime',
            controller: 'manualOutTimeController'
        })
        .when('/tiffin-bill', {
            templateUrl: 'humanresource/AttendanceManagement/TiffinBill',
            controller: 'attendanceManagementController'
        })
        .when('/manual-attendance-approval', {
            templateUrl: 'humanresource/ManualAttendanceConfirmation',
            controller: 'ManualAttendanceConfirmationController'
        })
        .when('/manual-day-status', {
            templateUrl: 'humanresource/attendanceProcessDataManualStatus',
            controller: 'attendanceProcessDataManualStatusController'
        })
        .when('/manual-day-status-new', {
            templateUrl: 'humanresource/attendanceProcessDataManualStatusNew',
            controller: 'attendanceProcessDataManualStatusNewController'
        })
        .when('/salary-structure-report',
            {
                templateUrl: 'humanresource/payrollReports/Aplos',
                controller: 'salaryStructureSheetController'
            })
        .when('/salary-structure-report-daily',
            {
                templateUrl: 'humanresource/payrollReports/SalaryStructureDaily',
                controller: 'salaryStructureSheetDailyController'
            })
        .when('/salary-structure-and-Processed-report',
            {
                templateUrl: 'humanresource/PayrollReports/SalaryStructureAndProcessedReport',
                controller: 'salaryStructureAndProcessedReportController'
            })
        .when('/salary-Processed-report',
            {
                templateUrl: 'humanresource/PayrollReports/SalaryProcessedReport',
                controller: 'salaryProcessedReportController'
            })
        .when('/arrear-Processed-report',
            {
                templateUrl: 'humanresource/PayrollReports/ArrearVsPayroll',
                controller: 'ArrearProcessedReportController'
            })
        .when('/arrear-Processed-report-total',
            {
                templateUrl: 'humanresource/PayrollReports/ArrearVsPayrollTotal',
                controller: 'ArrearProcessedTotalReportController'
            })
        .when('/bulk-leave-entry',
            {
                templateUrl: 'humanresource/BulkLeaveEntry',
                controller: 'BulkLeaveEntryController'
            })
        .when('/payregisternew',
            {
                templateUrl: 'humanresource/PayRegisterBDReport/PayRegisterNew',
                controller: 'payRegisterBDReportNewController'
            })
        .when('/payregistercom',
            {
                templateUrl: 'humanresource/PayRegisterBDReport/PayRegisterCom',
                controller: 'payRegisterBDReportComController'
            })
        .when('/payregistercontr',
            {
                templateUrl: 'humanresource/PayRegisterBDReport/PayRegisterContractor',
                controller: 'payRegisterBDReportContractorController'
            })
        .when('/leave-with-wages-registers', {
            templateUrl: 'humanresource/AttendanceManagement/LeaveWithWagesRegisters',
            controller: 'leaveWithWagesRegistersController'
        })
        .when('/leave-with-wages-registers-form-18', {
            templateUrl: 'humanresource/LeaveWithWagesRegistersForm18/Aplos',
            controller: 'leaveWithWagesRegistersForm18Controller'
        })
        .when('/welfare-reports', {
            templateUrl: 'humanresource/WelfareReports/Aplos',
            controller: 'welfareReportsController'
        })
        .when('/leaves-check-list-report', {
            templateUrl: 'humanresource/AttendanceManagement/leaveschecklistreport',
            controller: 'leavesChecklistReportController'
        })
        .when('/bonus-register', {
            templateUrl: 'humanresource/BonusRegisterReports',
            controller: 'bonusRegisterReportController'
        })
        .when('/ptax-report', {
            templateUrl: 'humanresource/ProfessionalTaxReports',
            controller: 'professionalTaxReportsController'
        })
        .when('/leave-wages-registers', {
            templateUrl: 'HumanResource/LeaveWithWeagesRegisters/Aplos',
            controller: 'leaveWithWagesRegistersController'
        })
        .when('/national-festival-leave', {
            templateUrl: 'humanresource/AttendanceManagement/NationalFestival',
            controller: 'nationalFestivalController'
        })
        .when('/preallocated-ot', {
            templateUrl: 'humanresource/preallocatedot/Aplos',
            controller: 'preallocatedOTController'
        })
        .when('/attendance-raw-data', {
            templateUrl: 'humanresource/AttendanceManagement/RawDataReport',
            controller: 'attendanceRawController'
        })

        .when('/pre-allocated-report', {
            templateUrl: 'humanresource/PreallocatedOT/preallocatedotreport',
            controller: 'preallocatedOTReportController'
        })

        .when('/tiffin-bill-report', {
            templateUrl: 'humanresource/AttendanceManagement/TiffinBillReport',
            controller: 'tiffinBillReportController'
        })

        .when('/tiffin-bill-summary-report', {
            templateUrl: 'humanresource/AttendanceManagement/TiffinBillSummaryReports',
            controller: 'tiffinBillReportSummaryController'
        })
        .when('/maternity-leave-report', {
            templateUrl: 'humanresource/AttendanceManagement/MaternityLeaveReport',
            controller: 'maternityLeaveReportController'
        })
        .when('/actual-ot-and-plan-report', {
            templateUrl: 'humanresource/AttendanceManagement/ActualOTAndPlan',
            controller: 'actualOTAndPlantController'
        })
        .when('/final-sattlement-report', {
            templateUrl: 'humanresource/AttendanceManagement/FinalSettlementReport',
            controller: 'finalSettlementReportController'
        })
        .when('/late-attendance-posting', {
            templateUrl: 'humanresource/AttendanceManagement/LateAttendancePosting',
            controller: 'lateAttendancePostingController'
        })
        .when('/separated-employee-salary-structure',
            {
                templateUrl: 'humanresource/payrollReports/SeparatedEmployeeSalaryStructure',
                controller: 'separatedsalaryStructureController'
            })

        .when('/daily-attendance-summary',
            {
                templateUrl: 'humanresource/DailyAttendanceSummary/Aplos',
                controller: 'dailyAttendanceSummaryController'
            })
        .when('/daily-attendance-summary-noline',
            {
                templateUrl: 'humanresource/DailyAttendanceSummary/DailyAttendanceSummaryNoLine',
                controller: 'dailyAttendanceSummaryNoLineController'
            })

        .when('/attendance-summary-status',
            {
                templateUrl: 'humanresource/AttendanceManagement/AttendanceSummaryStatus',
                controller: 'attendanceSummaryStatusController'
            })
        .when('/workers-Late-Status',
            {
                templateUrl: 'humanresource/AttendanceManagement/WorkersLateStatus',
                controller: 'workersLateStatusController'
            })
        .when('/shift-summary',
            {
                templateUrl: 'humanresource/ShiftSummary/Aplos',
                controller: 'shiftSummaryController'
            })
        .when('/ot-adjustment',
            {
                templateUrl: 'humanresource/OTAdjustment/Aplos',
                controller: 'OTAdjustmentController'
            })
        .when('/salary-lock',
            {
                templateUrl: 'humanresource/SalaryLock/Aplos',
                controller: 'salaryLockController'
            })
        .when('/salary-summary-report2',
            {
                templateUrl: 'humanresource/PayrollReports/SalarySummaryReport',
                controller: 'salaryProcessedReportSummaryController'
            })

        .when('/rest-type',
            {
                templateUrl: 'humanresource/RestType/Aplos',
                controller: 'RestTypeController'
            })
        .when('/manpowerbudget-dashboard', {
            templateUrl: 'humanresource/ManpowerBudgetDashboard/Aplos',
            controller: 'manpowerBudgetDashboardController'
        })
        .when('/plant-wise-hrms-setting', {
            templateUrl: 'humanresource/PlantWiseHRMSSetting/Aplos',
            controller: 'PlantWiseHRMSSettingController'
        })
        .when('/mp-budgeted-desig-report', {
            templateUrl: 'humanresource/ManpowerBudgetDesignationReport/Aplos',
            controller: 'manpowerBudgetDesignationReportController'
        })
        .when('/salary-sheet-budgetary-ot',
            {
                templateUrl: 'humanresource/PayrollReports/SalarySheetBudgetaryOT',
                controller: 'salaryProcessedReportBudgetaryController'
            })
        .when('/shift-change-section-wise', {
            templateUrl: 'humanresource/ShiftChangeSectionWise/Aplos',
            controller: 'ShiftChangeSectionWiseController'
        })
        .when('/salary-structure-report-plant-wise', {
            templateUrl: 'humanresource/PayrollReports/SalaryStructureReportPlantWise',
            controller: 'salaryStructureReportPlantWiseController'
        })
        .when('/daily-day-status-report', {
            templateUrl: 'humanresource/DailyDayStatusReport/Aplos',
            controller: 'DailyDayStatusReportController'
        })
        .when('/ot-planning', {
            templateUrl: 'humanresource/OTPlanning/Aplos',
            controller: 'OTPlanningController'
        })
        .when('/salary-proc-extctc', {
            templateUrl: 'humanresource/PayrollReports/SalaryProcessedReportExtraOTCTC',
            controller: 'salaryProcessedReportExtraOTCTCController'
        })
        .when('/salary-proc-extctc-original', {
            templateUrl: 'humanresource/PayrollReports/SalaryProcessedReportExtraOTCTC',
            controller: 'salaryProcessedReportExtraOTCTCOriginalController'
        })
        .when('/yearly-salarystatement-report', {
            templateUrl: 'humanresource/PayrollReports/YearlySalaryProcessedReport',
            controller: 'yearlySalaryProcessedReportController'
        })

        .when('/black-list', {
            templateUrl: 'humanresource/BlackList/',
            controller: 'BlackListController'
        })
        .when('/con-attdn-status', {
            templateUrl: 'humanresource/ConsecutiveAttendaceAndOT/',
            controller: 'consecutiveAttendaceController'
        })
        .when('/con-work-hours', {
            templateUrl: 'humanresource/ConsecutiveAttendaceAndOT/OTHours',
            controller: 'consecutiveOTHoursController'
        })
        .when('/bonus-form-c', {
            templateUrl: 'humanresource/BonusRegisterReports/BonusC',
            controller: 'bonusReportCController'
        })
        .when('/bonus-prv', {
            templateUrl: 'humanresource/BonusRegisterReports/BonusProvison',
            controller: 'bonusProvisionReportController'
        })
        .when('/employee-addition-deduction', {
            templateUrl: 'humanresource/EmployeeAdditionDeduction/Aplos',
            controller: 'EmployeeAdditionDeductionController'
        })
        .when('/manual-shift', {
            templateUrl: 'humanresource/ManualShift/Aplos',
            controller: 'ManualShiftController'
        })
        .when('/manual-shift-new', {
            templateUrl: 'humanresource/ManualShiftNew/Aplos',
            controller: 'ManualShiftNewController'
        })
        .when('/salary-proc-extctc-company', {
            templateUrl: 'humanresource/PayrollReports/SalaryProcessedReportExtraOTCTCCompany',
            controller: 'salaryProcessedReportExtraOTCTCCompanyController'
        })
        .when('/employee-addition-deduction-process', {
            templateUrl: 'humanresource/EmployeeAdditionDeductionProcess/Aplos',
            controller: 'EmployeeAdditionDeductionProcessController'
        })
        .when('/day-status-master', {
            templateUrl: 'humanresource/DayStatusMaster/Aplos',
            controller: 'DayStatusMasterController'
        })
        .when('/attdn-bonus-master', {
            templateUrl: 'humanresource/AttendanceBonusMaster/Aplos',
            controller: 'AttendanceBonusMasterController'
        })
        .when('/weekly-off', {
            templateUrl: 'humanresource/WeeklyOff/Aplos',
            controller: 'WeeklyOffController'
        })
        .when('/out-punch-configuration', {
            templateUrl: 'humanresource/OutPunchConfiguration/Aplos',
            controller: 'OutPunchConfigurationController'
        })
        .when('/ot-update-configuration', {
            templateUrl: 'humanresource/OTUpdateConfiguration/Aplos',
            controller: 'OTUpdateConfigurationController'
        })
        .when('/roster-pattern', {
            templateUrl: 'humanresource/RosterPattern/Aplos',
            controller: 'RosterPatternController'
        })
        .when('/emp-job-location', {
            templateUrl: 'humanresource/EmployeeJobLocation/Aplos',
            controller: 'EmployeeJobLocationController'
        })
        .when('/roster-updates', {
            templateUrl: 'humanresource/RosterUpdates/Aplos',
            controller: 'RosterUpdatesController'
        })
        .when('/weekoff-updates', {
            templateUrl: 'humanresource/WeekOffUpdates/Aplos',
            controller: 'WeekOffUpdatesController'
        })
        .when('/attnd-source-config', {
            templateUrl: 'humanresource/AttendanceSourceConfig/Aplos',
            controller: 'AttendanceSourceConfigController'
        })
        .when('/daily-in-status', {
            templateUrl: 'humanresource/NewAttdnDashboard/Aplos',
            controller: 'NewAttdnDashboardController'
        })
        .when('/admin-attdn-control', {
            templateUrl: 'humanresource/AdminAttendanceControl/',
            controller: 'AdminAttendanceControlController'
        })
        .when('/employee-budget-update', {
            templateUrl: 'humanresource/EmployeeBudgetUpdate/Aplos',
            controller: 'EmployeeBudgetUpdateController'
        })
        .when('/new-attdnprocess-lock', {
            templateUrl: 'humanresource/NewAttdnProcessLock/Aplos',
            controller: 'NewAttdnProcessLockController'
        })
        .when('/new-HRDashboard', {
            templateUrl: 'humanresource/NewHRDashboard/Aplos',
            controller: 'NewHRDashboardController'
        })
        .when('/credit-limit-opening', {
            templateUrl: 'humanresource/CreditLimitOpening/Aplos',
            controller: 'CreditLimitOpeningController'
        })
        .when('/tables-upload', {
            templateUrl: 'humanresource/TablesUpload/Aplos',
            controller: 'TablesUploadController'
        })
        .when('/audit-report-data-new', {
            templateUrl: 'humanresource/NewAttendanceProcessAuditReport/Aplos',
            controller: 'NewAttendanceProcessAuditReportController'
        })
        .when('/leave-app-new', {
            templateUrl: 'humanresource/LeaveApplicationNew/Aplos',
            controller: 'EmployeeLeaveApplicationNewController'
        })
        .when('/employee-leave-approval-new', {
            templateUrl: 'humanresource/EmployeeLeaveApprovalNew/Aplos',
            controller: 'EmployeeLeaveApprovalNewController'
        })
        .when('/salary-Processed-report-new',
            {
                templateUrl: 'humanresource/PayrollReports/SalaryProcessedReportNew',
                controller: 'salaryProcessedReportControllerNew'
            })
        .when('/salary-not-disbursed',
            {
                templateUrl: 'humanresource/PayrollReports/SalaryNotDisbursed',
                controller: 'SalaryNotDisbursedController'
            })
        .when('/salary-structure-and-Processed-report-new',
            {
                templateUrl: 'humanresource/PayrollReports/SalaryStructureAndProcessedReportNew',
                controller: 'salaryStructureAndProcessedReportNewController'
            })
        .when('/salary-integration-with-thirdparty',
            {
                templateUrl: 'humanresource/PayrollReports/SalaryIntegrationWithThirdParty',
                controller: 'SalaryIntegrationWithThirdPartyController'
            })

        .when('/leave-delete-new', {
            templateUrl: 'humanresource/LeaveApplicationNew/LeaveDelete',
            controller: 'employeeLeaveDeleteApplicationNewController'
        })
        .when('/employee-weekoff-updates', {
            templateUrl: 'humanresource/WeekOffUpdates/EWeekUpdate',
            controller: 'EmployeeWeekOffUpdatesController'
        })
        .when('/physical-verification-report', {
            templateUrl: 'humanresource/PhysicalVerificationReport/Aplos',
            controller: 'PhysicalVerificationReportController'
        })
        .when('/salary-disbursement-report', {
            templateUrl: 'humanresource/SalaryDisbursementReport/Aplos',
            controller: 'SalaryDisbursementReportController'
        })
        .when('/final-deduction',
            {
                templateUrl: 'humanresource/FinalDeductionReport/Aplos',
                controller: 'FinalDeductionReportController'
            })
        .when('/week-definition',
            {
                templateUrl: 'humanresource/WeekDefinition/Aplos',
                controller: 'WeekDefinitionController'
            })
        .when('/ot-confirmation-process',
            {
                templateUrl: 'humanresource/OTConfirmationProcess/Aplos',
                controller: 'OTConfirmationProcessController'
            })
        .when('/leaves-check-list-report-new', {
            templateUrl: 'humanresource/LeavesChecklistReportNew/Aplos',
            controller: 'LeavesChecklistReportNewController'
        })
        .when('/employee-shift-updates', {
            templateUrl: 'humanresource/EmployeeShiftUpdates/Aplos',
            controller: 'EmployeeShiftUpdatesController'
        })
        .when('/performance-management-master', {
            templateUrl: 'humanresource/PerformanceManagementMaster/Aplos',
            controller: 'PerformanceManagementMasterController'
        })
        .when('/performance-period', {
            templateUrl: 'humanresource/PerformancePeriod/Aplos',
            controller: 'PerformancePeriodController'
        })
        .when('/performance-group', {
            templateUrl: 'humanResource/PerformanceGroup/Aplos',
            controller: 'PerformanceGroupController'
        })
        .when('/performance-attribute-master', {
            templateUrl: 'humanResource/PerformanceAttributeMaster/Aplos',
            controller: 'PerformanceAttributeMasterController'
        })
        .when('/performance-grade-master', {
            templateUrl: 'humanResource/PerformanceGradeMaster/Aplos',
            controller: 'PerformanceGradeMasterController'
        })
        .when('/goal-setting-approval', {
            templateUrl: 'humanResource/GoalSettingApproval/Aplos',
            controller: 'GoalSettingApprovalController'
        })
        .when('/residence-master', {
            templateUrl: 'humanResource/ResidenceMaster/Aplos',
            controller: 'ResidenceMasterController'
        })
        .when('/scattered-week-off', {
            templateUrl: 'humanResource/ScatteredWeekOff/Aplos',
            controller: 'ScatteredWeekOffController'
        })
        .when('/residence-status-allocation', {
            templateUrl: 'humanResource/ResidenceStatusAllocation/Aplos',
            controller: 'ResidenceStatusAllocationController'
        })
        .when('/residence-status-allocation-report', {
            templateUrl: 'humanResource/ResidenceStatusAllocation/Report',
            controller: 'ResidenceStatusAllocationReportController'
        })
        .when('/absentism-reasoning-master', {
            templateUrl: 'humanResource/AbsentismReasoningMaster/Aplos',
            controller: 'AbsentismReasoningMasterController'
        })

        .when('/training-master', {
            templateUrl: 'humanResource/TrainingMaster/Aplos',
            controller: 'TrainingMasterController'
        })
        .when('/manpower-control-report', {
            templateUrl: 'humanResource/ManpowerControlReport/Aplos',
            controller: 'ManpowerControlReportsController'
        })

        .when('/fuguai-zone-master', {
            templateUrl: 'humanResource/FuguaiZoneMaster/Aplos',
            controller: 'FuguaiZoneMasterController'
        })
        .when('/fuguai-transaction', {
            templateUrl: 'humanResource/FuguaiTransaction/Aplos',
            controller: 'FuguaiTransactionController'
        })
        .when('/fuguai-report', {
            templateUrl: 'humanResource/FuguaiReport/Aplos',
            controller: 'FuguaiReportController'
        })
        .when('/employee-skill-matrix', {
            templateUrl: 'humanResource/EmployeeSkillMatrix/Aplos',
            controller: 'EmployeeSkillMatrixController'
        })
        .when('/ot-control-limit', {
            templateUrl: 'humanResource/OTControlLimit/Aplos',
            controller: 'OTControlLimitController'
        })
        .when('/ot-ctr-limit-report', {
            templateUrl: 'humanResource/OTControlLimit/Report',
            controller: 'OTControlLimitReportController'
        })

        .when('/ot-reason', {
            templateUrl: 'humanResource/OTRegion/Aplos',
            controller: 'OTRegionController'
        })

        .when('/ot-compensatory-allocation', {
            templateUrl: 'humanResource/OTCompensatoryAllocation/Aplos',
            controller: 'OTCompensatoryAllocationController'
        })
        .when('/furniture-master', {
            templateUrl: 'humanResource/FurnitureMaster/Aplos',
            controller: 'FurnitureMasterController'
        })
        .when('/furniture-policy', {
            templateUrl: 'humanResource/FurniturePolicy/Aplos',
            controller: 'FurniturePolicyController'
        })

        .when('/furniture-policy-report', {
            templateUrl: 'humanResource/FurniturePolicyReport/Aplos',
            controller: 'FurniturePolicyReportController'
        })

        .when('/5s-zone-master', {
            templateUrl: 'humanResource/FiveSZoneMaster/Aplos',
            controller: 'FiveSZoneMasterController'
        })

        .when('/survey-and-feedback', {
            templateUrl: 'humanResource/SurveyandFeedback/Aplos',
            controller: 'SurveyandFeedbackController'
        })

        .when('/medicine-master', {
            templateUrl: 'humanResource/MedicineMaster/Aplos',
            controller: 'MedicineMasterController'
        })

        .when('/sickness-type', {
            templateUrl: 'humanResource/SicknessType/Aplos',
            controller: 'SicknessTypeController'
        })

        .when('/medicine-purpose', {
            templateUrl: 'humanResource/MedicinePurpose/Aplos',
            controller: 'MedicinePurposeController'
        })

        .when('/medicine-receipt', {
            templateUrl: 'humanResource/MedicineReceipt/Aplos',
            controller: 'MedicineReceiptController'
        })
        .when('/medical-log', {
            templateUrl: 'humanResource/MedicalLog/Aplos',
            controller: 'MedicalLogController'
        })
        .when('/medical-log-report', {
            templateUrl: 'humanResource/MedicalLogReport/Aplos',
            controller: 'MedicalLogReportController'
        })

        .when('/medicine-category', {
            templateUrl: 'humanResource/MedicineCategory/Aplos',
            controller: 'MedicineCategoryController'
        })
        .when('/experience-master', {
            templateUrl: 'humanResource/ExperienceMaster/Aplos',
            controller: 'ExperienceMasterController'
        })
        .when('/employer-master', {
            templateUrl: 'humanResource/EmployerMaster/Aplos',
            controller: 'EmployerMasterController'
        })
        .when('/qualification-master', {
            templateUrl: 'humanResource/QualificationMaster/Aplos',
            controller: 'QualificationMasterController'
        })
        .when('/daily-attendance-status-report', {
            templateUrl: 'humanResource/DailyAttendanceStatusReport/Aplos',
            controller: 'DailyAttendanceStatusReportController'
        })
        .when('/leave-registers-form', {
            templateUrl: 'humanresource/LeaveRegistersForm/Aplos',
            controller: 'LeaveRegistersFormController'
        })
        .when('/employee-attendance-report', {
            templateUrl: 'humanresource/AttendanceReport/Report',
            controller: 'EmployeeAttendanceReportController'
        })

        .when('/designation-setup', {
            templateUrl: 'humanresource/DesignationSetup/Aplos',
            controller: 'DesignationSetupController'
        })

        .when('/hr-report-master', {
            templateUrl: 'humanresource/HRReportMaster/Aplos',
            controller: 'HRReportMasterController'
        })

        .when('/bgtcode-wise-hr-report', {
            templateUrl: 'humanresource/BudgetCodeWiseHRReport/Aplos',
            controller: 'BudgetCodeWiseHRReportController'
        })
        .when('/bgt-report-master', {
            templateUrl: 'humanresource/BudgetReportMaster/Aplos',
            controller: 'BudgetReportMasterController'
        })
        .when('/web-packing', {
            templateUrl: 'humanresource/WebBasedPacking/Aplos',
            controller: 'WebBasedPackingController'
        })
        .when('/salary-Processed-report-com',
            {
                templateUrl: 'humanresource/PayrollReports/SalaryProcessedReportCom',
                controller: 'salaryProcessedReportComController'
            })
        .when('/vehicle-movement-master',
            {
                templateUrl: 'humanresource/VehicleMovementMaster/Aplos',
                controller: 'VehicleMovementMasterController'
            })

        .when('/vehicle-movement-requisition',
            {
                templateUrl: 'humanresource/VehicleMovementMaster/VehicleMovementRequisition',
                controller: 'VehicleMovementRequisitionController'
            })

        .when('/vehicle-req-approval',
            {
                templateUrl: 'humanresource/VehicleMovementMaster/VehicleReqForApprove',
                controller: 'VehicleReqForApproveController'
            })
        .when('/vehicle-inout',
            {
                templateUrl: 'humanresource/VehicleMovementMaster/VehicleInOut',
                controller: 'VehicleInOutController'
            })

        .when('/vehicle-movement',
            {
                templateUrl: 'humanresource/VehicleMovementMaster/VehicleMovement',
                controller: 'VehicleMovementController'
            })

        .when('/vehicle-trip',
            {
                templateUrl: 'humanresource/VehicleMovementMaster/VehicleTrip',
                controller: 'VehicleTripController'
            })

        .when('/plant-inout',
            {
                templateUrl: 'humanresource/PlantInOutControllReport/Aplos',
                controller: 'PlantInOutControllReportController'
            })
        .when('/vehicle-report',
            {
                templateUrl: 'humanresource/VehicleMovementMaster/Vehiclereport',
                controller: 'VehicleReportController'
            })
        .when('/vehicle-report',
            {
                templateUrl: 'humanresource/OTConfirmationProcess/OTApprove',
                controller: 'otApproveController'
            })

        .when('/Leave-Transection',
            {
                templateUrl: 'humanresource/LeaveTransection/Aplos',
                controller: 'LeaveTransectionController'
            })
        .when('/ot-approve',
            {
                templateUrl: 'humanresource/OTConfirmationProcess/OTApprove',
                controller: 'otApproveController'
            })
        ;
}