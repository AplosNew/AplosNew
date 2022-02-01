MeetingManagementConfig.$inject = ['$routeProvider', '$locationProvider'];
function MeetingManagementConfig($routeProvider, $locationProvider) {
    $routeProvider
        .when('/meeting-category', {
            templateUrl: 'MeetingManagement/MeetingCategory',
            controller: 'MeetingCategoryController'
        })

        .when('/meeting-sub-category', {
            templateUrl: 'MeetingManagement/MeetingSubCategory',
            controller: 'MeetingSubCategoryController'
        })

        .when('/meeting-type', {
            templateUrl: 'MeetingManagement/MeetingType',
            controller: 'MeetingTypeController'
        })




};