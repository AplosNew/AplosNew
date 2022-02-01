MeetingManagementConfig.$inject = ['$routeProvider', '$locationProvider'];
function MeetingManagementConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/characteristics', {
            templateUrl: 'Materials/characteristics',
            controller: 'characteristicsController'
        })








};