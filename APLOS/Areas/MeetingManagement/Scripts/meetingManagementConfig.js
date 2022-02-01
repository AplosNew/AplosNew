MeetingManagementConfig.$inject = ['$routeProvider', '$locationProvider'];
function MeetingManagementConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/meeting-category', {
            templateUrl: 'MeetingManagement/MeetingCategory',
            controller: 'MeetingCategoryController'
        })








};