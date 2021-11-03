function EmployeeServicesConfig($routeProvider, $locationProvider) {
    $routeProvider
        
        .when('/emp-service-type', {
            templateUrl: 'EmployeeServices/EmployeeServiceType/Aplos',
            controller: 'EmployeeServiceTypeController'
        })

        .when('/emp-service-booking', {
            templateUrl: 'EmployeeServices/EmployeeServiceBooking/Aplos',
            controller: 'EmployeeServiceBookingController'
        })

        .when('/emp-service-rate', {
            templateUrl: 'EmployeeServices/EmployeeServicesRate/Aplos',
            controller: 'EmployeeServicesRateController'
        })
       
}
EmployeeServicesConfig.$inject = ['$routeProvider', '$locationProvider'];