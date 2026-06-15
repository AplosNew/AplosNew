function qmsConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/qms-activity-master', {
            templateUrl: 'QMS/ActivityMaster/Aplos',
            controller: 'ActivityMasterController'
        })
        .when('/defect-check-level', {
            templateUrl: 'QMS/DefectCheckLevel/aplos',
            controller: 'DefectCheckLevelController'
        })
        .when('/defect-type', {
            templateUrl: 'QMS/DefectType/aplos',
            controller: 'DefectTypeController'
        })

        .when('/document-type', {
            templateUrl: 'QMS/DocumentType/aplos',
            controller: 'documentTypeController'
        })
        .when('/qms-activity-category', {
            templateUrl: 'QMS/QMSActivityCategory/aplos',
            controller: 'qmsActivityCategoryController'
        })
        .when('/quality-activity-checkType', {
            templateUrl: 'QMS/QualityActivityCheckType/aplos',
            controller: 'qualityActivityCheckTypeController'
        })
        .when('/business-process', {
            templateUrl: 'QMS/QMSBusinessProcess/aplos',
            controller: 'qmsBusinessProcessController'
        })
        .when('/business-process-type', {
            templateUrl: 'QMS/QMSBusinessProcessType/aplos',
            controller: 'qmsBusinessProcessTypeController'
        })
        .when('/document-source', {
            templateUrl: 'QMS/DocumentSource/aplos',
            controller: 'DocumentSourceController'
        })
       
        .when('/repair-type', {
            templateUrl: 'QMS/RepairType/aplos',
            controller: 'RepairTypeController'
        })
        
        .when('/defect-zone', {
            templateUrl: 'QMS/DefectZone/aplos',
            controller: 'DefectZoneController'
        })
        .when('/qmstesting-master', {
            templateUrl: 'QMS/QMSTestingMaster/aplos',
            controller: 'QMSTestingMasterController'
        })
        .when('/inspection-master', {
            templateUrl: 'QMS/InspectionMaster/aplos',
            controller: 'InspectionMasterController'
        })
        .when('/process-parameter', {
            templateUrl: 'QMS/ProcessParameter/aplos',
            controller: 'ProcessParameterController'
        })
        .when('/document-location', {
            templateUrl: 'QMS/DocumentLocation/Aplos',
            controller: 'DocumentLocationController'
        })

        

        .when('/qms-defect-master', {
            templateUrl: 'QMS/QMSDefectMaster/Aplos',
            controller: 'QMSDefectMasterController'
        })

        .when('/qms-master', {
            templateUrl: 'QMS/QMSMaster/Aplos',
            controller: 'QMSMasterController'
        })

        .when('/inspection-type', {
            templateUrl: 'QMS/InspectionType/Aplos',
            controller: 'InspectionTypeController'
        })

        .when('/qms-inspection', {
            templateUrl: 'QMS/QMSInspection/Aplos',
            controller: 'QMSInspectionController'
        })

        .when('/quality-status', {
            templateUrl: 'QMS/QualityStatus/Aplos',
            controller: 'QualityStatusController'
        })

        .when('/stock-keeping-unit', {
            templateUrl: 'QMS/StockKeepingUnit/Aplos',
            controller: 'StockKeepingUnitController'
        })

        .when('/grade-master', {
            templateUrl: 'QMS/GradeMaster/Aplos',
            controller: 'GradeMasterController'
        })

        .when('/qms-rejection', {
            templateUrl: 'QMS/QMSRejection/Aplos',
            controller: 'QMSRejectionController'
        })

        .when('/issue', {
            templateUrl: 'QMS/Issue/Aplos',
            controller: 'IssueController'
        })

        .when('/issue-master', {
            templateUrl: 'QMS/IssueMaster/Aplos',
            controller: 'IssueMasterController'
        })

        .when('/quality-process', {
            templateUrl: 'QMS/QualityProcess/aplos',
            controller: 'QualityProcessController'
        })

        .when('/qlty-process', {
            templateUrl: 'QMS/QualityProcess/aplosnew',
            controller: 'QualityProcessNewController'
        })

        .when('/quality-management-master', {
            templateUrl: 'QMS/QualityManagementMaster/aplos',
            controller: 'QualityManagementMasterController'
        })

        .when('/quality-setup', {
            templateUrl: 'QMS/QualitySetup/Aplos',
            controller: 'QualitySetupController'
        })

        .when('/document-setup', {
            templateUrl: 'QMS/DocumentSetup/Aplos',
            controller: 'DocumentSetupController'
        })
        .when('/complaint-master', {
            templateUrl: 'QMS/Complaint/Aplos',
            controller: 'ComplaintController'
        })
        .when('/customer-quality-and-technical-support', {
            templateUrl: 'QMS/CustomerQualityAndTechnicalSupport/Aplos',
            controller: 'CustomerQualityAndTechnicalSupportController'
        })
        .when('/product-parameter-master', {
            templateUrl: 'QMS/ProductParameterMaster/aplos',
            controller: 'ProductParameterMasterController'
        })
        .when('/process-parameter-master', {
            templateUrl: 'QMS/ProcessParameterMaster/aplos',
            controller: 'ProcessParameterMasterController'
        })
        .when('/define-process-parameter', {
            templateUrl: 'QMS/DefineProcessParameter/aplos',
            controller: 'DefineProcessParameterController'
        })
        .when('/customer-requirement-control', {
            templateUrl: 'QMS/CustomerRequirementControl/aplos',
            controller: 'CustomerRequirementControlController'
        })
        .when('/customer-confirmation-parameter', {
            templateUrl: 'QMS/CustomerConfirmationParameter/aplos',
            controller: 'CustomerConfirmationParameterController'
        })
        .when('/customer-completed-parameter', {
            templateUrl: 'QMS/CustomerCompletedParameter/aplos',
            controller: 'CustomerCompletedParameterController'
        })
        .when('/parameter-setting-control', {
            templateUrl: 'QMS/ParameterSettingControl/aplos',
            controller: 'ParameterSettingControlController'
        })
        .when('/order-wise-quality-report', {
            templateUrl: 'QMS/OrderWiseQualityReport/aplos',
            controller: 'OrderWiseQualityReportController'
        })
        .when('/daily-quality-status-report', {
            templateUrl: 'QMS/DailyQualityStatusReport/aplos',
            controller: 'DailyQualityStatusReportController'
        })
        .when('/sqc-master', {
            templateUrl: 'QMS/SQCMaster/aplos',
            controller: 'SQCMasterController'
        })
        .when('/define-sqc-issue', {
            templateUrl: 'QMS/DefineSQCIssue/AplosWC',
            controller: 'DefineSQCIssueController'
        })
        .when('/lot-wise-quality-report', {
            templateUrl: 'QMS/LotWiseQualityReport/Aplos',
            controller: 'LotWiseQualityReportController'
        })
        .when('/lot-wise-quality-summary-report', {
            templateUrl: 'QMS/LWQSummaryReport/Aplos',
            controller: 'LWQSummaryReportController'
        })
        .when('/lot-wise-quality-report-edit', {
            templateUrl: 'QMS/LWQRUpdate/Aplos',
            controller: 'LWQRUpdateController'
        })
        .when('/defect-marker', {
            templateUrl: 'QMS/QualityProcess/DefectMarker',
            controller: 'DefectMarkerController'
        })
        .when('/image-master', {
            templateUrl: 'QMS/QualityProcess/ImageMaster',
            controller: 'ImageMasterController'
        })
        .when('/image-inspec-type', {
            templateUrl: 'QMS/QualityProcess/ImageInspectionType',
            controller: 'ImageInspectionTypeController'
        })
    

}
qmsConfig.$inject = ['$routeProvider', '$locationProvider'];