employeeConfig.$inject = ['$routeProvider', '$locationProvider', '$windowProvider'];
function employeeConfig($routeProvider, $locationProvider, $windowProvider) {
    $routeProvider
        .when('/job-description-category', {
            templateUrl: 'employees/jobdescriptioncategory/',
            controller: 'jobDescriptionCategoryController'
        })
        .when('/job-description-subcategory', {
            templateUrl: 'employees/jobdescriptionsubcategory/',
            controller: 'jobDescriptionSubCategoryController'
        })
        .when('/job-description-item', {
            templateUrl: 'employees/jobdescriptionitem/',
            controller: 'jobDescriptionItemController'
        })
        .when('/qualification-level', {
            templateUrl: 'employees/qualificationlevel/',
            controller: 'qualificationLevelController'
        })
        .when('/qualification-stream', {
            templateUrl: 'employees/qualificationstream/',
            controller: 'qualificationStreamController'
        })
        .when('/job-description', {
            templateUrl: 'employees/jobdescription/',
            controller: 'jobDescriptionController'
        })
        .when('/recruitment-process', {
            templateUrl: 'employees/recruitmentprocess/',
            controller: 'recruitmentProcessController'
        })
        .when('/recruitment-process-set', {
            templateUrl: 'employees/recruitmentprocessset/',
            controller: 'recruitmentProcessSetController'
        })
        .when('/recruitment-group', {
            templateUrl: 'employees/recruitmentgroup/',
            controller: 'recruitmentGroupController'
        })
        .when('/blood-group', {
            templateUrl: 'employees/bloodgroup/',
            controller: 'bloodGroupController'
        })
        .when('/civil-status', {
            templateUrl: 'employees/civilstatus/',
            controller: 'civilStatusController'
        })
        .when('/recruitment-selection', {
            templateUrl: 'employees/recruitmentselection/',
            controller: 'recruitmentSelectionController'
        })
        .when('/recruitment-approval', {
            templateUrl: 'employees/recruitmentapproval/',
            controller: 'recruitmentApprovalController'
        })
        .when('/interview-ranking', {
            templateUrl: 'employees/interviewranking/',
            controller: 'interviewRankingController'
        })
        .when('/salutation', {
            templateUrl: 'employees/salutation/',
            controller: 'salutationController'
        })
        .when('/compliance-Document-Category', {
            templateUrl: 'employees/complianceDocumentCategory/',
            controller: 'complianceDocumentCategoryController'
        })
        .when('/compliance-Document-subcategory', {
            templateUrl: 'employees/complianceDocumentSubCategory/',
            controller: 'complianceDocumentSubCategoryController'
        })
        .when('/compliance-Document', {
            templateUrl: 'employees/complianceDocument/Aplos',
            controller: 'complianceDocumentController'
        })
        .when('/documentConfiguration-DesignationGroup', {
            templateUrl: 'employees/documentConfigurationDesignationGroup/',
            controller: 'documentConfigurationDesignationGroupController'
        })
        .when('/compliance-DocumentSet', {
            templateUrl: 'employees/complianceDocumentSet/',
            controller: 'complianceDocumentSetController'
        })
        .when('/payment-mode-change', {
            templateUrl: 'employees/PaymentModeChange/',
            controller: 'paymentModeChangeController'
        })
        .when('/recruitment-appdataedit', {
            templateUrl: 'employees/recruitmentappdataedit/',
            controller: 'recruitmentAppDataEditController'
        })
        .when('/compliance-Document-report', {
            templateUrl: 'employees/complianceDocument/ComplianceDocumentReportPage',
            controller: 'complianceDocumentReportController'
        })
        .when('/employee-job-description', {
            templateUrl: 'employees/employeejobdescription/',
            controller: 'employeeJobDescriptionController'
        })
        .when('/employee-budget-category-department', {
            templateUrl: 'employees/employeebudgetcategorydepartment/',
            controller: 'employeeBudgetCategoryDepartmentController'
        })
        .when('/employee-category', {
            templateUrl: 'employees/employeecategory/',
            controller: 'employeeCategoryController'
        })
        .when('/employee-status', {
            templateUrl: 'employees/employeestatus/',
            controller: 'employeesStatusController'
        })
        .when('/employee-budget-category', {
            templateUrl: 'employees/employeebudgetcategory/',
            controller: 'employeeBudgetCategoryController'
        })
        .when('/employee-group', {
            templateUrl: 'employees/employeegroup/',
            controller: 'employeeGroupController'
        })
        .when('/employee-information', {
            templateUrl: 'employees/employeeinformation/',
            controller: 'employeeInformationController'
        })
        .when('/employee-register', {
            templateUrl: 'employees/employeeinformation/empregister',
            controller: 'employeeRegisterController'
        })
        .when('/employee-info-report', {
            templateUrl: 'Employees/employeeinformation/EmpInfo',
            controller: 'employeeReportInfoController'
        })
        .when("/employee-ledger", {
            templateUrl: "Employees/EmployeeReport/EmployeeLedgerReport",
            controller: "employeeLedgerReportController"
        })
        .when("/employee-ob-ledger", {
            templateUrl: "Employees/EmployeeReport/EmployeeLedgerOpeningBalanceReport",
            controller: "employeeLedgerOpeningBalanceReportController"
        })
        .when('/employee-expense-booking-report', {
            templateUrl: 'Employees/EmployeeReport/EmployeeExpenseBookingReport',
            controller: 'employeeExpenseBookingReportController'
        })
        .when('/employee-budget-master', {
            templateUrl: 'Employees/EmployeeInformation/budgetmaster',
            controller: 'employeeBudgetMasterController'
        })
        .when('/employee-budget-master/:id/:name', {
            templateUrl: 'employees/EmployeeInformation/budgetmaster',
            controller: 'employeeBudgetMasterController'
        })
        .when('/employee-budget-master-activity/:employeeId/:employeeName/:budgetMasterId/:budgetMasterName', {
            templateUrl: 'employees/EmployeeInformation/budgetmasterActivity',
            controller: 'employeeBudgetMasterActivityController'
        })
        .when('/employee-budget-master-activity-phone/:employeeId/:employeeName/:budgetMasterId/:budgetMasterName/:activityId/:activityName', {
            templateUrl: 'employees/EmployeeInformation/budgetmasterActivityPhone',
            controller: 'employeeBudgetMasterActivityPhoneController'
        })
        .when('/approved-employee', {
            templateUrl: 'employees/approvedemployee/',
            controller: 'approvedEmployeeController'
        })
        .when('/employee-leave-balance', {
            templateUrl: 'employees/employeeleavebalance/',
            controller: 'employeeLeaveBalanceController'
        })
        .when('/employee-leave-carry-forward', {
            templateUrl: 'employees/employeeleavecarryforward/',
            controller: 'employeeLeaveCarryForwardController'
        })
        .when('/employee-document-assignment', {
            templateUrl: 'employees/employeedocumentassignment/',
            controller: 'employeeDocumentAssignmentController'
        })
        .when('/employee-document-report', {
            templateUrl: 'employees/DocumentDashboard/EmployeeDocumentReport',
            controller: 'documentDashboardController'
        })
        .when('/employee-salary-editable', {
            templateUrl: 'employees/EmployeeSalaryRuleEditable',
            controller: 'employeeSalaryRuleEditableController'
        })
        .when('/employee-idcard-print', {
            templateUrl: 'employees/employeeIdCard/IdCard',
            controller: 'employeeIdCardController'
        })
        .when('/multiple-idcard', {
            templateUrl: 'employees/employeeIdCard/Aplos',
            controller: 'multipleIdCardController'
        })
        .when('/budgetcode-change', {
            templateUrl: 'employees/BudgetCodeChange/Aplos',
            controller: 'budgetCodeChangeController'
        })
        .when('/approval-configuration', {
            templateUrl: 'employees/approvalconfiguration/',
            controller: 'approvalConfigurationController'
        })
        .when('/resignation', {
            templateUrl: 'employees/resignation/',
            controller: 'resignationController'
        })
        .when('/multiple-resignation-approval', {
            templateUrl: 'employees/ResignationApprovalMultiple/',
            controller: 'multipleResignationApprovalController'
        })
        .when('/resignation-approval', {
            templateUrl: 'employees/resignationapproval/',
            controller: 'resignationApprovalController'
        })
        .when('/probational-period', {
            templateUrl: 'employees/employeeprobationalperiod/',
            controller: 'employeeProbationalPeriodController'
        })
        .when('/prerecruitment-documentby-department', {
            templateUrl: 'employees/prerecruitmentdocumentbydepartment/',
            controller: 'preRecruitmentDocumentByDepartmentController'
        })
        .when('/document-dashboard', {
            templateUrl: 'employees/documentdashboard/',
            controller: 'documentDashboardController'
        })
        .when('/prerecruitment-document-approval', {
            templateUrl: 'employees/prerecruitmentdocumentapproval/',
            controller: 'preRecruitmentDocumentApprovalController'
        })
        .when('/direct-tax-payment-head', {
            templateUrl: 'employees/directtaxpaymenthead/',
            controller: 'directTaxPaymentHeadController'
        })
        .when('/stoppage', {
            templateUrl: 'employees/stoppage/',
            controller: 'stoppageController'
        })
        .when('/route', {
            templateUrl: 'employees/route/',
            controller: 'routeController'
        })
        .when('/routeemployee', {
            templateUrl: 'employees/routeemployee/Aplos',
            controller: 'routeEmployeeController'
        })
        .when('/route-employee-report', {
            templateUrl: 'employees/routeemployee/Report',
            controller: 'RouteEmployeeReportController'
        })
        .when('/postrecruitment-documentby-department', {
            templateUrl: 'employees/postrecruitmentdocumentbydepartment/',
            controller: 'postRecruitmentDocumentByDepartmentController'
        })
        .when('/resignation-recruitment-planning', {
            templateUrl: 'employees/resignationrecruitmentplanning/',
            controller: 'resignationRecruitmentPlanningController'
        })
        .when('/candidate-administration', {
            templateUrl: 'employees/candidateadministration/',
            controller: 'candidateAdministrationController'
        })
        .when('/employeebankinformation', {
            templateUrl: 'employees/employeebankinformation/',
            controller: 'employeeBankInformationController'
        })
        .when('/pre-rec-dashboard', {
            templateUrl: 'Recruitments/predashboard/',
            controller: 'dashBoardController'
        })
       
        .when('/compliance-document-proof-type', {
            templateUrl: 'employees/compliancedocumentprooftype/',
            controller: 'complianceDocumentProofTypeController'
        })
        .when('/hr-dashboard', {
            templateUrl: 'employees/HRDashboard',
            controller: 'hrDashboardController'
        })
        .when('/hr-dashboardtr', {
            templateUrl: 'employees/HRDashboardtr',
            controller: 'hrDashboardtrController'
        })
        .when('/sop-document', {
            templateUrl: 'employees/sopdocument',
            controller: 'sopDocumentController'
        })
        .when('/sop-document-category', {
            templateUrl: 'employees/sopdocumentcategory',
            controller: 'sopDocumentCategoryController'
        })
        .when('/sop-document-subcategory', {
            templateUrl: 'employees/sopdocumentsubcategory',
            controller: 'sopDocumentSubCategoryController'
        })
        .when('/sop-category', {
            templateUrl: 'employees/sopcategory',
            controller: 'sopCategoryController'
        })
        .when('/sop-subcategory', {
            templateUrl: 'employees/sopsubcategory',
            controller: 'sopSubCategoryController'
        })
        .when('/sop-item', {
            templateUrl: 'employees/sopitem',
            controller: 'sopItemController'
        })
        .when('/pFEmployee-applied', {
            templateUrl: 'employees/PFEmployeeApplied',
            controller: 'pFEmployeeAppliedController'
        })
        .when('/employeedocumentaddremove', {
            templateUrl: 'Employees/EmployeeInformation/EmployeeDocumentAddRemove',
            controller: 'employeedocumentAddRemoveController'
        })
        .when('/recruitment', {
            templateUrl: 'employees/recruitment',
            controller: 'recruitmentController'
        })
        .when('/pFEmployee-VoluntaryValue', {
            templateUrl: 'employees/PFEmployeeVoluntaryValue',
            controller: 'pFEmployeeVoluntaryValueController'
        })
        .when('/leave-opening-balance', {
            templateUrl: 'employees/LeaveOpeningBalance',
            controller: 'leaveOpeningBalanceController'
        })
        .when('/candidate-document-assignment', {
            templateUrl: 'employees/candidateadministration/candidatedocument',
            controller: 'candidateDocumentAssignmentController'
        })
        .when('/candidatedocumentaddremove', {
            templateUrl: 'employees/candidateadministration/candidatedocumentaddremove',
            controller: 'candidatedocumentAddRemoveController'
        })
        .when('/weekly-absentism-assignment', {
            templateUrl: 'employees/WeeklyAbsentismAssignment',
            controller: 'weeklyAbsentismAssignmentController'
        })
        .when('/department-responsible-person', {
            templateUrl: 'employees/departmentresponsibleperson',
            controller: 'departmentResponsiblePersonController'
        })
        .when('/bonus-eligible-applied', {
            templateUrl: 'employees/BonusPolicyMonthlyRetainEligibleEmployee',
            controller: 'BonusPolicyMonthlyRetainEligibleEmployeeController'
        })
        .when('/leave-apply', {
            templateUrl: 'employees/leaveApplication/LeaveApply',
            controller: 'SectionemployeeLeaveApplicationController'
        })
        .when('/leave-app', {
            templateUrl: 'employees/leaveApplication/LeaveApp',
            controller: 'employeeLeaveApplicationController'
        })
        .when('/leave-delete', {
            templateUrl: 'employees/leaveApplication/LeaveDelete',
            controller: 'employeeLeaveDeleteApplicationController'
        })
        .when('/individual-compliance', {
            templateUrl: 'employees/employeeinformation/IndividualComplianceReport',
            controller: 'individualComplianceReportController'
        })
        .when('/holiday-absenteeism', {
            templateUrl: 'employees/holidayabsentismassignment/aplos',
            controller: 'holidayAbsentismAssignmentController'
        })

        .when('/skill-Matrix', {
            templateUrl: 'employees/SkillMatrix/aplos',
            controller: 'skillMatrixController'
        })
        .when('/emp-ActiveInActive', {
            templateUrl: 'employees/EmpActiveInActive/aplos',
            controller: 'empActiveInActiveController'
        })
        .when('/emp-ActiveInActive-new', {
            templateUrl: 'employees/EmpActiveInActiveNew/aplos',
            controller: 'empActiveInActiveNewController'
        })
        .when('/compensatory-off', {
            templateUrl: 'Employees/CompensatoryOff/aplos',
            controller: 'CompensatoryOffController'
        })
        .when('/compensatory-off-new', {
            templateUrl: 'Employees/CompensatoryOffNew/aplos',
            controller: 'CompensatoryOffNewController'
        })
        .when('/exception-for-holiday', {
            templateUrl: 'Employees/ExceptionForHoliday/aplos',
            controller: 'ExceptionForHolidayController'
        })
        .when('/lay-off-assign', {
            templateUrl: 'Employees/LayOff/aplos',
            controller: 'LayOffController'
        })
        .when('/mediasoft-fairshop', {
            templateUrl: 'employees/employeeinformation/MediasoftFairShopDataExport',
            controller: 'individualComplianceReportController'
        })
        .when('/separation-Type', {
            templateUrl: 'Employees/SeparationType/aplos',
            controller: 'separationtypeController'
        })
        .when('/employee-lock', {
            templateUrl: 'employees/employeeinformation/EmployeeLockAndUnLock',
            controller: 'EmployeeLockAndUnLockController'
        })
        .when('/employee-profile-approval', {
            templateUrl: 'employees/EmployeeProfileApproval/EmployeeProfileApproval',
            controller: 'EmployeeProfileApprovalController'
        })
        .when('/employee-profile-unapproval', {
            templateUrl: 'employees/EmployeeProfileUnApproval/Aplos',
            controller: 'EmployeeProfileUnApprovalController'
        })
        .when('/od-delete', {
            templateUrl: 'employees/ODDelete/ODDelete',
            controller: 'oDDeleteController'
        })
        .when('/od-delete-new', {
            templateUrl: 'employees/ODDeleteNew/Aplos',
            controller: 'oDDeleteNewController'
        })
        .when('/exception-employee', {
            templateUrl: 'employees/ExceptionEmployee/Aplos',
            controller: 'exceptionEmployeeController'
        })
        .when('/authorization', {
            templateUrl: 'employees/AuthorizationConfig/Aplos',
            controller: 'authorizationConfigController'
        })
        .when('/allowance-daily', {
            templateUrl: 'employees/AllowanceDaily/Aplos',
            controller: 'allowanceDailyController'
        })
        .when('/employee-worktype', {
            templateUrl: 'employees/employeeworktype/Aplos',
            controller: 'employeeWorkTypeController'
        })
        .when('/employee-delete', {
            templateUrl: 'employees/EmployeeDelete/Aplos',
            controller: 'employeeDeleteController'
        })
        .when('/update-dos', {
            templateUrl: 'employees/EmployeeDelete/UpdateDOS',
            controller: 'updateEmployeeDOSController'
        })
        .when('/employee-doj-change', {
            templateUrl: 'employees/EmployeeDOJChange/Aplos',
            controller: 'EmployeeDOJChangeController'
        })
        .when('/guest-user', {
            templateUrl: 'employees/guestuser/Aplos',
            controller: 'guestUserController'
        })
        .when('/qr-code-employee', {
            templateUrl: 'employees/QRCodeGenerationEmployee/Aplos',
            controller: 'QRCodeGenerationEmployeeController'
        })
        .when('profile-upload', {
            templateUrl: 'Employees/EmployeeInformation/ProfileUpload',
            controller:"ProfileFromExcelController"
        })
        .when('/emp-information-report', {
            templateUrl: 'employees/EmployeeInFoReport/Aplos',
            controller: "employeeInFoReportController"
        })
        .when('/salaryhead-gl', {
            templateUrl: 'employees/SalaryHeadGL/Aplos',
            controller: "salaryHeadGLController"
        })
        .when('/employee-information-new', {
            templateUrl: 'employees/employeeinformation/Aplos1',
            controller: 'employeeInformationNewController'
        })
        .when('/employee-plant-transfer', {
            templateUrl: 'Employees/EmployeePlantTransfer/Aplos',
            controller: 'EmployeePlantTransferController'
        })
        .when('/employee-plant-transfer-new', {
            templateUrl: 'Employees/EmployeePlantTransferNew/Aplos',
            controller: 'EmployeePlantTransferNewController'
        })
        .when('/company-wise-plant-transfer', {
            templateUrl: 'Employees/CompanyWiseEmployeePlantTransfer/Aplos',
            controller: 'CompanyWiseEmployeePlantTransferController'
        })
        .when('/special-unlock', {
            templateUrl: 'Employees/SpecialUnlock/Aplos',
            controller: 'specialUnlockController'
        })
        .when('/leave-delete-singleday', {
            templateUrl: 'Employees/LeaveDeleteSingleDay/Aplos',
            controller: 'LeaveDeleteSingleDayController'
        })
        .when('/leave-delete-singleday-new', {
            templateUrl: 'Employees/LeaveDeleteSingleDayNew/Aplos',
            controller: 'LeaveDeleteSingleDayNewController'
        })
        .when('/accounts-group', {
            templateUrl: 'Employees/AccountsGroup/Aplos',
            controller: 'AccountsGroupController'
        })
        .when('/empcode-gen', {
            templateUrl: 'Employees/EmployeeCodeGeneration/Aplos',
            controller: 'EmployeeCodeGenerationController'
        })
        .when('/residence-group', {
            templateUrl: 'Employees/ResidenceGroup/Aplos',
            controller: 'ResidenceGroupController'
        })
        .when('/transport-group', {
            templateUrl: 'Employees/TransportGroup/Aplos',
            controller: 'TransportGroupController'
        })
        .when('/multiple-resignation-approval-new', {
            templateUrl: 'employees/ResignationApprovalMultipleNew/Aplos',
            controller: 'multipleResignationApprovalNewController'
        })
        .when('/empcode-type', {
            templateUrl: 'Employees/EmployeeCodeType/Aplos',
            controller: 'EmployeeCodeTypeController'
        })
        .when('/document-category', {
            templateUrl: 'Employees/DocumentCategory/Aplos',
            controller: 'DocumentCategoryController'
        })

        .when('/resignation-type', {
            templateUrl: 'Employees/ResignationType/Aplos',
            controller: 'ResignationTypeController'
        })
        .when('/myapp-employee-ledger', {
            templateUrl: 'Employees/EmployeeReport/MyappEmployeeLedgerReport',
            controller: 'myappEmployeeLedgerReportController'
        })
        .when('/caste', {
            templateUrl: 'Employees/Caste/Aplos',
            controller: 'CasteController'
        })
        .when('/bulk-upload', {
            templateUrl: 'Employees/EmployeeInformation/BulkUpload',
            controller: 'EmployeeBulkUploadFNFController'
        })
        .when('/report', {
            templateUrl: 'Employees/EmployeeInFoReport/Report',
            controller: 'userDefineReportController'
        })
        .when('/trend-report', {
            templateUrl: 'Employees/EmployeeInFoReport/Trend',
            controller: 'employeeAttdnTrendReportController'
        })
        .when('/filter', {
            templateUrl: 'Employees/EmployeeInFoReport/Filter',
            controller: 'favouriteReportController'
        })
        .when('/user-filter', {
            templateUrl: 'Employees/EmployeeInFoReport/UserFilter',
            controller: 'userfavouriteReportController'
        })

        .when('/attend-verification-report', {
            templateUrl: 'Employees/EmployeeInFoReport/AttendVerificationStatus',
            controller: 'AttendanceVerificationStatusController'
        })
        .when('/dailytarget', {
            templateUrl: 'Employees/EmployeeInformation/DailyTargetUpload',
            controller: 'DailyTargetUploadController'
        })
        ;
} 