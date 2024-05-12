SetupConfig.$inject = ['$routeProvider', '$locationProvider'];
function SetupConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/brand', {
            templateUrl: 'Setups/brand',
            controller: 'brandController'
        })
        .when('/language', {
            templateUrl: 'Setups/language',
            controller: 'languageController'
        })
        .when('/local-language-label', {
            templateUrl: 'Setups/LocalLanguage/Label',
            controller: 'localLanguageLabelController'
        })
        .when('/local-language-master-entry', {
            templateUrl: 'Setups/LocalLanguage/aplos',
            controller: 'localLanguageSalaryHeadController'
        })
        .when('/payment-mode', {
            templateUrl: 'Setups/paymentmode',
            controller: 'paymentModeController'
        })
        .when('/payment-term', {
            templateUrl: 'accounts/paymentterm',
            controller: 'paymentTermController'
        })
        .when('/unit-of-measurement', {
            templateUrl: 'Setups/unitofmeasurement',
            controller: 'unitOfMeasurementController'
        })
        .when('/yearly-calendar', {
            templateUrl: 'Setups/yearlycalendar',
            controller: 'yearlyCalendarController'
        })
        .when('/weekend', {
            templateUrl: 'Setups/offdaymaster/weekend',
            controller: 'offDayMasterController'
        })
        .when('/holiday', {
            templateUrl: 'Setups/offdaymaster/holiday',
            controller: 'holidayCalendarController'
        })
        .when('/entity-Calendar', {
            templateUrl: 'Setups/entityCalendar',
            controller: 'entityCalendarController'
        })
        .when('/plant-config', {
            templateUrl: 'Setups/plantconfig',
            controller: 'plantConfigController'
        })
        .when('/prdord-setting', {
            templateUrl: 'Setups/prdOrdSetting',
            controller: 'prdOrdSettingController'
        })
        .when('/uom-conversion', {
            templateUrl: 'Setups/uomconversion',
            controller: 'uOMConversionController'
        })
        .when('/religion', {
            templateUrl: 'Setups/religion',
            controller: 'religionController'
        })
        .when('/sales-type', {
            templateUrl: 'Setups/salestype',
            controller: 'salesTypeController'
        })
        .when('/service-type', {
            templateUrl: 'Setups/servicetype',
            controller: 'serviceTypeController'
        })
        .when('/service-group', {
            templateUrl: 'Setups/servicegroup',
            controller: 'serviceGroupController'
        })
        .when('/company-service-master', {
            templateUrl: 'Setups/companyServiceMaster',
            controller: 'companyServiceMasterController'
        })
        .when('/service-master', {
            templateUrl: 'Setups/servicemaster',
            controller: 'serviceMasterController'
        })
        .when('/service-control', {
            templateUrl: 'Setups/servicemaster/serviceControl',
            controller: 'serviceControlController'
        })
        .when('/hsn-code', {
            templateUrl: 'Setups/hsncode',
            controller: 'hsnCodeController'
        })
        .when('/country-hsn-code', {
            templateUrl: 'Setups/countryhsncode',
            controller: 'countryHSNCodeController'
        })
        .when('/testing-category', {
            templateUrl: 'Setups/testingcategory',
            controller: 'testingCategoryController'
        })
        .when('/testing', {
            templateUrl: 'Setups/testing',
            controller: 'testingController'
        })
        .when('/testing-standard', {
            templateUrl: 'Setups/testingstandard',
            controller: 'testingStandardController'
        })
        .when('/testing-standard-report', {
            templateUrl: 'Setups/testingstandard/testingstandardreportpage',
            controller: 'testingStandardReportController'
        })
        .when('/hsn-tax-percentage', {
            templateUrl: 'Setups/HSNTaxPercentageNew',
           //controller: 'hSNTaxPercentageController'
            controller: 'hSNTaxPercentageControllerNew'
        })
        .when('/business-process', {
            templateUrl: 'Setups/businessprocess',
            controller: 'businessProcessController'
        })
        .when('/define-enum', {
            templateUrl: 'Setups/businessprocess/DefineEnum',
            controller: 'defineEnumController'
        })
        .when('/mail-receiver', {
            templateUrl: 'Setups/mailReceiver',
            controller: 'mailReceiverController'
        })
        .when('/mail-receiver-service', {
            templateUrl: 'Setups/mailReceiver/MailRecipientService',
            controller: 'mailReceiverServiceController'
        })
        .when('/administrative-mail-reciepient-service', {
            templateUrl: 'Setups/mailReceiver/AdministrativeMailRecipient',
            controller: 'administrativeMailReceiverServiceController'
        })
        .when('/holiday-category', {
            templateUrl: 'Setups/holidayCategory',
            controller: 'holidayCategoryController'
        })
        .when('/process-uom', {
            templateUrl: 'Setups/ProcessUoM',
            controller: 'processUoMController'
        })
        .when('/mail-send', {
            templateUrl: 'Setups/MailSend',
            controller: 'mailSendController'
        })
        .when('/plantwisetermsandconditions', {
            templateUrl: 'Setups/plantWiseTermsAndConditions',
            controller: 'plantWiseTermsAndConditionsController'
        })
        .when('/plantwiselettertemplate', {
            templateUrl: 'Setups/plantwiselettertemplate',
            controller: 'plantWiseLetterTemplateController'
        })
        .when('/retention-allowance', {
            templateUrl: 'Setups/RetentionAllowance',
            controller: 'retentionAllowanceController'
        })
        .when('/service-group-accountdeterminate', {
            templateUrl: 'Setups/ServiceGroupGL/',
            controller: 'serviceGroupGLController'
        })
        .when('/buyer-activity', {
            templateUrl: 'Setups/OrderActivity/BuyerActivity',
            controller: 'buyerActivityController'
        })
        .when('/inquiry-activity', {
            templateUrl: 'Setups/OrderActivity/InquiryActivity',
            controller: 'inquiryActivityController'
        })
        .when('/employee-location', {
            templateUrl: 'Setups/EmployeeLocation',
            controller: 'employeeLocationController'
        })
        .when('/shift-group', {
            templateUrl: 'Setups/ShiftGroup',
            controller: 'shiftGroupController'
        })
        .when('/shift-group-detail', {
            templateUrl: 'Setups/ShiftGroup/ShiftGroupDetail',
            controller: 'shiftGroupDetailController'
        })
        .when('/relationship', {
            templateUrl: 'Setups/relationship',
            controller: 'relationshipController'
        })
        .when('/profession', {
            templateUrl: 'Setups/profession',
            controller: 'professionController'
        })
        .when('/plantsetting', {
            templateUrl: 'Setups/plantsetting',
            controller: 'plantSettingController'
        })
        .when('/tnasetting-master', {
            templateUrl: 'Setups/TnaSettingMaster/Aplos',
            controller: 'tnaSettingMasterController'
        })
        .when('/custom-weekend', {
            templateUrl: 'setups/customweekend/aplos',
            controller: 'customWeekendController'
        })
        .when('/rpt-template', {
            templateUrl: 'setups/rptconfigtemplate/aplos',
            controller: 'rptConfigTemplateController'
        })
        .when('/attendance-group', {
            templateUrl: 'setups/AttendanceGroup/aplos',
            controller: 'attendanceGroupController'
        })
        .when('/employee-attendance-group', {
            templateUrl: 'setups/EmployeeAttendanceGroup/aplos',
            controller: 'employeeAttendanceGroupController'
        })
        .when('/special-tax', {
            templateUrl: 'setups/specialtax/aplos',
            controller: 'specialTaxController'
        })
        .when('/entity-config', {
            templateUrl: 'setups/entityconfig/aplos',
            controller: 'entityConfigController'
        })
        .when('/notification-setting', {
            templateUrl: 'setups/notificationsetting/aplos',
            controller: 'notificationSettingController'
        })
        .when('/reporting-group', {
            templateUrl: 'setups/ReportingGroup/aplos',
            controller: 'ReportingGroupController'
        })
        .when('/label-list', {
            templateUrl: 'setups/LabelList/aplos',
            controller: 'LabelListController'
        })
        .when('/remarks-control', {
            templateUrl: 'setups/RemarksControl/aplos',
            controller: 'RemarksControlController'
        })
        ;
}