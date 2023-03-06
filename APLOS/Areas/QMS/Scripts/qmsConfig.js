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

        .when('/quality-management-master', {
            templateUrl: 'QMS/QualityManagementMaster/aplos',
            controller: 'QualityManagementMasterController'
        })
        

}
qmsConfig.$inject = ['$routeProvider', '$locationProvider'];