//BiometricConfig.$inject = ['$routeProvider', '$locationProvider'];
//function BiometricConfig($routeProvider, $locationProvider) {
//    $routeProvider
//        .when('/AttendanceDeviceZone', {
//            templateUrl: 'Biometric/AttrendanceDeviceZone/Aplos',
//            controller: 'attendanceDeviceZoneController'
//        })

//        ;
//}

BiometricConfig.$inject = ['$routeProvider', '$locationProvider'];
function BiometricConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/AttendanceDeviceZone', {
            templateUrl: 'Biometric/AttendanceDeviceZone/',
            controller: 'AttendanceDeviceZoneController'
        })

        ;
}