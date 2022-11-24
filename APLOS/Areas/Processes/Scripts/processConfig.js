ProcessConfig.$inject = ['$routeProvider'];
function ProcessConfig($routeProvider) {
    $routeProvider
        .when('/process-category', {
            templateUrl: 'Processes/processcategory',
            controller: 'processCategoryController'
        })
        .when('/process', {
            templateUrl: 'Processes/process',
            controller: 'processController'
        })
        .when('/company-process', {
            templateUrl: 'Processes/companyprocess',
            controller: 'companyProcessController'
        })
        .when('/sub-process', {
            templateUrl: 'Processes/subprocess',
            controller: 'subProcessController'
        })
        .when('/company-sub-process', {
            templateUrl: 'Processes/companysubprocess',
            controller: 'companySubProcessController'
        })
        .when('/sub-process-category', {
            templateUrl: 'Processes/subprocesscategory',
            controller: 'subProcessCategoryController'
        })
        .when('/process-config', {
            templateUrl: 'Processes/processconfig',
            controller: 'processConfigurationController'
        })
        .when('/entity-process-tag', {
            templateUrl: 'Processes/entityprocesstag',
            controller: 'entityProcessTagController'
        })
        .when('/utility', {
            templateUrl: 'Processes/utility',
            controller: 'utilityController'
        })
        .when('/process-criteria', {
            templateUrl: 'Processes/processcriteria',
            controller: 'processCriteriaController'
        })
        .when('/process-set', {
            templateUrl: 'Processes/Processset',
            controller: 'processSetController'
        })
        .when('/process-set-report', {
            templateUrl: 'Processes/processset/processsetreportpage',
            controller: 'processSetReportController'
        })
        .when('/sub-process-set', {
            templateUrl: 'Processes/subprocessset',
            controller: 'subProcessSetController'
        })
        .when('/process-type', {
            templateUrl: 'Processes/Processtype',
            controller: 'processTypeController'
        })
        .when('/production-process-group', {
            templateUrl: 'Processes/ProductionProcessGroup',
            controller: 'productionProcessGroupController'
        })
        .when('/process-group', {
            templateUrl: 'Processes/processGroup',
            controller: 'processGroupController'
        })
        .when('/prod-book-process-param', {
            templateUrl: 'Processes/ProductionBookingProcessparameter',
            controller: 'ProductionBookingProcessparameterController'
        })
        ;
}
