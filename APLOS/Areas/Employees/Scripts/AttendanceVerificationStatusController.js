
'use strict';
AttendanceVerificationStatusController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AttendanceVerificationStatusController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Attendance Verification Status Report";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Employees/EmployeeInFoReport/';
    $scope.getListUrl = $scope.path + 'getlist';

    $scope.fromDate = $filter('dateFiltering')(Date.now());
    $scope.toDate = $filter('dateFiltering')(Date.now());
    $scope.IncentiveType = '';
    $scope.EntityId = '';
    $scope.shiftId = '';
    $scope.workCenterId = '';
    $scope.dayStatus = '';
    $scope.ReportFormat = 'Excel';

    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });
  
    $scope.AttendVerificationStatusReport = function () {
        try {
            if (angular.isUndefinedOrNull($scope.fromDate))
                throw 'Plase select from date.';
            if (angular.isUndefinedOrNull($scope.toDate))
                throw 'Plase select to date.';
            var file_src = $scope.path + 'GetAttendanceVerificationStatusReport?reportFormat=' + $scope.ReportFormat + '&fromDate=' + $scope.fromDate + '&toDate=' + $scope.toDate + '&yesNo='+ true;
            $rootScope.report(file_src);
        } catch (e) {

        }
    }
  
}