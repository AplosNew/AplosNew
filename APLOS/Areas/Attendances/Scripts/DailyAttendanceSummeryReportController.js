'use strict';
DailyAttendanceSummeryReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function DailyAttendanceSummeryReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Daily Attendance Summary";
    $scope.path = 'Attendances/DailyAttendanceSummeryReport/';


 

    $scope.GetdailyattendanceSummeryReport = function () {
        var reportFormat = "Excel";
        try {
            var url = 'Attendances/DailyAttendanceSummeryReport/GetReport?reportFormat=' + reportFormat + "&WorkDate=" + $scope.WorkDate ;

            $rootScope.report(url);
        } catch (e) {

        }
    };
}