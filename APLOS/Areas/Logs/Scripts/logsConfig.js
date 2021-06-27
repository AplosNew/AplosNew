LogsConfig.$inject = ['$routeProvider', '$locationProvider'];
function LogsConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/access-log', {
            templateUrl: 'Logs/accesslog',
            controller: 'accessLogController'
        })
        .when('/action-log', {
            templateUrl: 'Logs/actionlog',
            controller: 'actionLogController'
        })
        .when('/error-log', {
            templateUrl: 'Logs/errorlog',
            controller: 'errorLogController'
        })
       .when('/mail-log', {
        templateUrl: 'Logs/MailLog',
        controller: 'mailLogController'
      });
};