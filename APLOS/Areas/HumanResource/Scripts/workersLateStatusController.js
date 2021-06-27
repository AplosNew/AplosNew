'use strict';
workersLateStatusController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function workersLateStatusController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Workers Late Status';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });

    ////$scope.Enitylist = [];
    ////$scope.EntityCbo = function () {
    ////    $http.get('HumanResource/AttendanceManagement/EntityCbo')
    ////        .then(
    ////            function successCallback(response) {
    ////                if (baseService.arrayLength(response.data) > 0) {
    ////                    $scope.Enitylist = [];
    ////                    $scope.Enitylist = response.data;
    ////                }
    ////            },
    ////            function errorCallback(response) {
    ////                ShowResult(response, 'failure');
    ////            });
    ////};
    ////$scope.EntityCbo();


    $scope.WorkerLateStatusReport = {

        WorkDate: $filter('dateFiltering')(Date.now()),
        ReportFormat: 'Excel'
    };

    $scope.WorkerLateStatusReportData = function () {
        try {
           // $scope.UserName = $("#ID option:selected").text();
            if (baseService.isUndefinedOrNull($scope.WorkerLateStatusReport.WorkDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("Work Date  is required.", 'failure');
            }

            else {
                var url = 'HumanResource/AttendanceManagement/GetWorkerLateStatusReport?reportFormat=Excel' + ' &WorkDate=' + $scope.WorkerLateStatusReport.WorkDate /*+ ' &EntityId=' + $scope.WorkerLateStatusReport.EntityId + ' &EntityUserName=' + $scope.UserName*/;
                $rootScope.report(url);
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}