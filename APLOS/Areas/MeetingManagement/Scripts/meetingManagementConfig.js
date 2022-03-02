MeetingManagementConfig.$inject = ['$routeProvider', '$locationProvider'];
function MeetingManagementConfig($routeProvider, $locationProvider) {
    $routeProvider
        
        .when('/meeting-type', {
            templateUrl: 'MeetingManagement/MeetingType',
            controller: 'MeetingTypeController'
        })

        .when('/meeting-agenda', {
            templateUrl: 'MeetingManagement/MeetingAgenda/Aplos',
            controller: 'MeetingAgendaController'
        })
        .when('/meeting-reports', {
            templateUrl: 'MeetingManagement/MeetingReports/Aplos',
            controller: 'MeetingReportsController'
        })
};