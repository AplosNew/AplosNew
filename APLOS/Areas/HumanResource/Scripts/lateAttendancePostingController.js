'use strict';
lateAttendancePostingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function lateAttendancePostingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Late Attendance Posting';


    $scope.AttendancePosting = {
        MonthId: null,
        ReportFormat: 'Excel'
    };

    $('.datepicker').datepicker({
        autoclose: true,
        minViewMode: 1,
        format: 'MM-yyyy'
    });

    $scope.AttendancePostingData = function () {
        try {      
            $scope.uom = $("#IdMonth option:selected").text();
            var url = 'HumanResource/AttendanceManagement/GetLateAttendancePostingReport?reportFormat=Excel' + ' &EffectiveDate=' + $scope.AttendancePosting.EffectiveDate;
                $rootScope.report(url);         
        } catch (e) {
            ShowResult(e, 'failure');

        }
    };
}