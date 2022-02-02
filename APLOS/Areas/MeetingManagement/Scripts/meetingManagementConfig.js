MeetingManagementConfig.$inject = ['$routeProvider', '$locationProvider'];
function MeetingManagementConfig($routeProvider, $locationProvider) {
    $routeProvider
        
        .when('/meeting-type', {
            templateUrl: 'MeetingManagement/MeetingType',
            controller: 'MeetingTypeController'
        })




};