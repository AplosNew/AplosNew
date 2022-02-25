AdministrationConfig.$inject = ['$routeProvider', '$locationProvider'];
function AdministrationConfig($routeProvider, $locationProvider) {
    $routeProvider
        
        .when('/vehicle-movement-locations', {
            templateUrl: 'Administration/VehicleMovementLocations',
            controller: 'VehicleMovementLocationsController'
        })

        .when('/services-approving-authority', {
            templateUrl: 'Administration/ServicesApprovingAuthority',
            controller: 'ServicesApprovingAuthorityController'
        })

        .when('/visitor-list-report', {
            templateUrl: 'Administration/VisitorListReport',
            controller: 'VisitorListReportController'
        })
};