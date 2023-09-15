'use strict';
BtbPerformanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function BtbPerformanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'BTB Performance';
    $scope.path = 'Commercial/LCReports/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.BTBPerformanceList = [];
    $scope.getAdditionalData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetBTBPerformanceDataList'
        }).then(function successCallback(response) {
            $scope.BTBPerformanceList = response.data;
        });
    }
    $scope.getAdditionalData();

    $scope.BTBPerformanceReport = function () {
        var dataList = [];
        var g = $("#btbPerGrid").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.BTBPerformanceList;
        }
        $scope.fileName = 'BTB Performance Report.xlsx';

        $http({
            method: 'POST',
            url: $scope.path + "BTBPerformanceDataXls",
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
}


