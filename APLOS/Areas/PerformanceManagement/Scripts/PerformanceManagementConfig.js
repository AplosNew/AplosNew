function PerformanceManagementConfig($routeProvider, $locationProvider) {
    $routeProvider
        
        .when('/job-evaluation-attribute', {
            templateUrl: 'PerformanceManagement/JobEvaluationAttribute/Aplos',
            controller: 'JobEvaluationAttributeController'
        })

        .when('/job-evaluation-master', {
            templateUrl: 'PerformanceManagement/JobEvaluationMaster/Aplos',
            controller: 'JobEvaluationMasterController'
        })

        .when('/job-evaluation', {
            templateUrl: 'PerformanceManagement/JobEvaluation/Aplos',
            controller: 'JobEvaluationController'
        })

        .when('/job-evaluation-report', {
            templateUrl: 'PerformanceManagement/JobEvaluationReport/Aplos',
            controller: 'JobEvaluationReportController'
        })
        .when('/manpower-control-report', {
            templateUrl: 'PerformanceManagement/ManpowerControlReport/Aplos',
            controller: 'ManpowerControlReportController'
        })
}
PerformanceManagementConfig.$inject = ['$routeProvider', '$locationProvider'];