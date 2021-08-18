'use strict';
AttendanceDashboardController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function AttendanceDashboardController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'Attendance Dashboard';
    $scope.path = 'Attendances/AttendanceDashboard/';
}