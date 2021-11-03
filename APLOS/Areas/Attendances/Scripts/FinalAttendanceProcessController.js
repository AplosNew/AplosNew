'use strict';
FinalAttendanceProcessController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function FinalAttendanceProcessController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Monthly Attendance Summary";
    $scope.path = 'Attendances/FinalAttendanceProcess/';

    $scope.AttendanceProcess = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "FinalAttendance",
                data: { 'fromDate': $scope.FromDate, 'toDate': $scope.ToDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'info');
        }
    };

}