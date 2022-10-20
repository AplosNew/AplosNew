MachineConfig.$inject = ['$routeProvider', '$locationProvider'];
function MachineConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/machine-class', {
            templateUrl: 'Machines/machineclass',
            controller: 'machineClassController'
        })
        .when('/operation', {
            templateUrl: 'Machines/operation',
            controller: 'operationController'
        })
        .when('/operation-type', {
            templateUrl: 'Machines/operationtype',
            controller: 'operationTypeController'
        })
        .when('/operation-variation', {
            templateUrl: 'Machines/operationVariation',
            controller: 'operationVariationController'
        })
        .when('/third-party-operation', {
            templateUrl: 'Machines/thirdpartyoperation',
            controller: 'thirdPartyOperationController'
        })
        .when('/operation-category', {
            templateUrl: 'Machines/operationcategory',
            controller: 'operationCategoryController'
        })
        .when('/operation-activity', {
            templateUrl: 'Machines/operationActivity',
            controller: 'operationActivityController'
        })
        .when('/entity-operation-settings', {
            templateUrl: 'Machines/entityOperationSettings',
            controller: 'entityOperationSettingsController'
        })
        .when('/machine-variant', {
            templateUrl: 'Machines/MachineVariant',
            controller: 'machineVariantController'
        })
        .when('/operation-motion', {
            templateUrl: 'Machines/operationMotion',
            controller: 'operationMotionController'
        })
        .when('/stitch-code', {
            templateUrl: 'Machines/stitchCode',
            controller: 'stitchCodeController'
        })
        .when('/production-system', {
            templateUrl: 'Machines/productionSystem',
            controller: 'productionSystemController'
        })
        .when('/machine-attribute', {
            templateUrl: 'Machines/MachineAttribute',
            controller: 'machineAttributeController'
        })
        .when('/machine-category', {
            templateUrl: 'machines/machinecategory/aplos',
            controller: 'machineCategoryController'
        })
        .when('/machine-subcategory', {
            templateUrl: 'machines/machinesubcategory/aplos',
            controller: 'machineSubCategoryController'
        })

        //.when('/machine-master', {
        //    templateUrl: 'Machines/MachineMaster/aplos',
        //    controller: 'machineMasterController'
        //})

        .when('/machine-maintenance-scheduling', {
            templateUrl: 'Machines/MaintenanceScheduling/Aplos',
            controller: 'maintenanceSchedulingController'
        })

        .when('/maintenance-status-details', {
            templateUrl: 'Machines/MaintenanceStatusDetails/Aplos',
            controller: 'maintenanceStatusDetailsController'
        })

        .when('/pending-maintenance-schedule', {
            templateUrl: 'Machines/PendingMaintenanceSchedule/Aplos',
            controller: 'pendingMaintenanceScheduleController'
        })

        .when('/special-issue-control', {
            templateUrl: 'Machines/SpecialIssueControl/Aplos',
            controller: 'specialIssueControlController'
        })

        .when('/issue-control-update', {
            templateUrl: 'Machines/SpecialIssueControlUpdate/Aplos',
            controller: 'specialIssueControlUpdateController'
        })

        .when('/issue-control-register', {
            templateUrl: 'Machines/SpecialIssueControlRegister/Aplos',
            controller: 'specialIssueControlRegisterController'
        })

        .when('/machine-masters', {
            templateUrl: 'Machines/MachineMas/aplos',
            controller: 'machineMasterControllers'
        })


        ;


   
       

    
}


