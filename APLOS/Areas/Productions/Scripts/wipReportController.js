'use strict';
wipReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function wipReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "WIP Report";
    $scope.path = 'Productions/WIPReport/';
    $scope.wipDate = $filter('dateFiltering')(Date.now());
    $scope.prdProcessSetList = [];
    $scope.ProcessId = null;

    $scope.GetProcessList = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetProcessList"
        }).then(function successCallback(response) {
            $scope.prdProcessSetList = response.data;
        });
    }
    $scope.GetProcessList();

    $scope.GetWIPReport = function () {
        try {
            $rootScope.report('Productions/WIPReport/GetWipReport?date=' + $scope.wipDate);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.GetWIPReportPivot = function () {
        try {
            $rootScope.report('Productions/WIPReport/GetWipReportPivot?date=' + $scope.wipDate);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetWIPReportWrokcenterWise = function () {
        try {
            $rootScope.report('Productions/WIPReport/GetWipReportProcessWise?ProcessId=' + $scope.ProcessId + '&date=' + $scope.wipDate);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    
    
}