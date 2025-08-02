IEConfig.$inject = ['$routeProvider', '$locationProvider'];
function IEConfig($routeProvider, $locationProvider) {
    $routeProvider
        // IE time-capture
        .when('/time-capture', {
            templateUrl: 'IE/timecapture/aplos',
            controller: 'timeCaptureController'
        })
        // IE subsection-structure
        .when('/subsection-structure', {
            templateUrl: 'IE/subsectionstructure/aplos',
            controller: 'subsectionStructureController'
        })
        // IE bulletin
        .when('/bulletin', {
            templateUrl: 'IE/bulletin/aplos',
            controller: 'bulletinController'
        })
        // IE Operation-Video-Upload
        .when('/operation-video-upload', {
            templateUrl: 'IE/operationvideoupload/aplos',
            controller: 'operationVideoUploadController'
        })
        .when('/size-group', {  //use this url.
            templateUrl: 'IE/sizegroup/aplos',
            controller: 'sizeGroupController'
        })


        .when('/attachment', {
            templateUrl: 'IE/attachment/aplos',
            controller: 'attachmentController'
        })

        .when('/gauge-folder', {
            templateUrl: 'IE/gaugefolder/aplos',
            controller: 'gaugeFolderController'
        })

        .when('/operation-consumption', {
            templateUrl: 'IE/operationconsumption/aplos',
            controller: 'operationConsumptionController'
        })


        .when('/operation-master', {
            templateUrl: 'IE/OperationMaster/aplos',
            controller: 'OperationMasterController'
        })


        .when('/machine-masterUI', {
            templateUrl: 'IE/MachineMasterUI/aplos',
            controller: 'machineMasterUIController'
        })

        .when('/machine-masterTransaction', {
            templateUrl: 'IE/MachineMasterTransaction/aplos',
            controller: 'MachineMasterTransactionController'
        })

        .when('/machine-masterTransactionReport', {
            templateUrl: 'IE/MachineMasterTransactionReport/Aplos',
            controller: 'MachineMasterTransactionReportController'
        })

        .when('/detention-type', {
            templateUrl: 'IE/DetentionType/Aplos',
            controller: 'DetentionTypeController'
        })

        .when('/skill-grouping', {
            templateUrl: 'IE/SkillGrouping/aplos',
            controller: 'skillGroupingController'
        })

        .when('/bulletin-template', {
            templateUrl: 'ie/bulletintemplate/aplos',
            controller: 'bulletinTemplateController'
        })
        .when('/line-designer', {
            templateUrl: 'ie/LineDesigner/aplos',
            controller: 'LineDesignerController'
        })
        .when('/qr-code-operation', {
            templateUrl: 'ie/QRCodeGenerationOperation/Aplos',
            controller: 'QRCodeGenerationOperationController'
        })
        .when('/AdditionalElement-code', {
            templateUrl: 'IE/sewingcode/aplos',
            controller: 'sewingCodeController'
        })
        .when('/element-code', {
            templateUrl: 'IE/elementcode/aplos',
            controller: 'elementCodeController'
        })
        .when('/production-allowance', {
            templateUrl: 'IE/productionsystemallowance/aplos',
            controller: 'productionSystemAllowanceController'
        })
        .when('/element-type', {
            templateUrl: 'IE/vaselementtype/aplos',
            controller: 'vASElementTypeController'
        })
        .when('/bartack-code', {
            templateUrl: 'IE/bartackcode/aplos',
            controller: 'bartackCodeController'
        })
        .when('/vas-approval', {
            templateUrl: 'IE/vasapproval/aplos',
            controller: 'vasApprovalController'
        })
        .when('/sam-compare', {
            templateUrl: 'IE/vassamcompare/aplos',
            controller: 'vasSAMCompareController'
        })
        .when('/vas-report', {
            templateUrl: 'IE/vasreport/aplos',
            controller: 'vasReportController'
        })
  
        .when('/bulletin-report', {
            templateUrl: 'ie/BulletinReport/Aplos',
            controller: 'bulletinReportController'
        })
        .when('/additional-element-code-settings', {
            templateUrl: 'ie/AdditionalElementCodeSettings/Aplos',
            controller: 'AdditionalElementCodeSettingsController'
        })
        .when('/skill-mapping', {
            templateUrl: 'ie/SkillMap/Aplos',
            controller: 'SkillMapController'
        })
        .when('/machine-map', {
            templateUrl: 'ie/MachineMap/Aplos',
            controller: 'MachineMapController'
        })
        .when('/line-layout-for-production-bulletin', {
            templateUrl: 'ie/LineLayoutForProductionBulletin/Aplos',
            controller: 'LineLayoutForProductionBulletinController'
        })
        .when('/workcenter-wise-detention', {
            templateUrl: 'ie/WorkcenterWiseDetention/Aplos',
            controller: 'WorkcenterWiseDetentionController'
        })
        .when('/IncentiveType', {
            templateUrl: 'ie/IncentiveType/Aplos',
            controller: 'IncentiveTypeController'
        })
        ;
};