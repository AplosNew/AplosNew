AdministrationConfig.$inject = ['$routeProvider', '$locationProvider'];
function AdministrationConfig($routeProvider, $locationProvider) {
    $routeProvider
        
        .when('/vehicle-movement-locations', {
            templateUrl: 'Administration/VehicleMovementLocations',
            controller: 'VehicleMovementLocationsController'
        })      
};