IEConfig.$inject = ['$routeProvider', '$locationProvider'];
function IEConfig($routeProvider, $locationProvider) {
    $routeProvider
        // IE time-capture
        .when('/daily-production-display', {
            templateUrl: 'IE/DailyProductionDisplay/aplos',
            controller: 'DailyProductionDisplayController'
        })
       
        .when('/line-designer', {
            templateUrl: 'ie/LineDesigner/aplos',
            controller: 'LineDesignerController'
        })
        
        ;
};