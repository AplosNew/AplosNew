ModuleConfig.$inject = ['$routeProvider', '$locationProvider'];
function ModuleConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/module', {
            templateUrl: 'Modules/Module/Aplos',
            controller: 'moduleController'
        })
        .when('/sub-module', {
            templateUrl: 'Modules/Module/SubModule',
            controller: 'subModuleController'
        })
        .when('/module-extended', {
            templateUrl: 'Modules/Module/ModuleExtended',
            controller: 'moduleExtendedController'
        })
        .when('/prerecruitment-url', {
            templateUrl: 'Modules/Module/PrerecruitmentUrl',
            controller: 'prerecruitmentUrlController'
        })
        .when('/company-group-module', {
            templateUrl: 'Modules/Module/CompanyGroupModule',
            controller: 'companyGroupModuleController'
        })
        .when('/module-app', {
            templateUrl: 'Modules/Module/ModuleApp',
            controller: 'moduleAppController'
        })
        .when('/company-group-module-app', {
            templateUrl: 'Modules/Module/CompanyGroupModuleApp',
            controller: 'companyGroupModuleAppController'
        })
        .when('/notification-url', {
            templateUrl: 'Modules/Module/NotificationURL',
            controller: 'notificationURLController'
        });
}